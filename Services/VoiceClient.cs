using Concentus;
using Concentus.Enums;
using Microsoft.Extensions.Configuration;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace isegoria_wpf.Services
{
    public class VoiceClient
    {
        private static readonly VoiceClient _instance = new VoiceClient();
        public static VoiceClient Instance => _instance;

        // ─── 오디오 관련 상수 정의 ───
        private const int SAMPLE_RATE = 48000;
        private const int CHANNELS = 1;
        private const int BITS_PER_SAMPLE = 16;
        private const int FRAME_MS = 20;
        private const int FRAME_SAMPLES = SAMPLE_RATE * FRAME_MS / 1000;                       // 960
        private const int FRAME_BYTES = FRAME_SAMPLES * CHANNELS * (BITS_PER_SAMPLE / 8);     // 1920
        private const int MAX_OPUS_BUFFER = 1275;                                              // Opus 최대 페이로드 크기
        private const int UDP_HEADER_BYTES = 48;                                               // 고정 헤더 크기

        private UdpClient? _udpClient;
        private IPEndPoint? _serverEndPoint;
        private CancellationTokenSource? _cts;
        private long _currentChannelId;

        private WaveInEvent? _waveIn;
        private WaveOutEvent? _waveOut;
        private MixingSampleProvider? _mixer;

        private IOpusEncoder? _encoder;
        private uint _sequenceNum;
        private uint _timestamp;

        private readonly Dictionary<long, UserVoiceStream> _userStreams = new Dictionary<long, UserVoiceStream>();
        private readonly List<byte> _pcmAccumulator = new List<byte>();

        private bool _isMuted;
        private bool _isDeafened;

        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                if (_isMuted != value)
                {
                    _isMuted = value;
                    if (RealtimeClient.Instance.IsConnected)
                    {
                        _ = RealtimeClient.Instance.SendPacketAsync(new
                        {
                            type = "SET_MUTE",
                            muted = _isMuted
                        });
                    }
                }
            }
        }

        public bool IsDeafened
        {
            get => _isDeafened;
            set
            {
                if (_isDeafened != value)
                {
                    _isDeafened = value;
                    if (RealtimeClient.Instance.IsConnected)
                    {
                        _ = RealtimeClient.Instance.SendPacketAsync(new
                        {
                            type = "SET_DEAFEN",
                            deafened = _isDeafened
                        });
                    }
                }
            }
        }

        public long CurrentChannelId => _currentChannelId;

        private VoiceClient()
        {
            RealtimeClient.Instance.OnPacketReceived += (type, json) =>
            {
                if (type == "VOICE_STATE")
                {
                    if (json.TryGetProperty("userId", out var userProp) &&
                        json.TryGetProperty("joined", out var joinedProp))
                    {
                        long userId = userProp.GetInt64();
                        bool joined = joinedProp.GetBoolean();

                        if (!joined)
                        {
                            RemoveUserStream(userId);
                        }
                    }
                }
            };
        }

        public async Task JoinVoiceChannelAsync(long channelId)
        {
            if (_currentChannelId == channelId) return;

            await LeaveVoiceChannelAsync();

            _currentChannelId = channelId;
            _cts = new CancellationTokenSource();

            try
            {
                _udpClient = new UdpClient(0);
                int localPort = ((IPEndPoint)_udpClient.Client.LocalEndPoint).Port;

                var config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();

                string host = config["Realtime:Host"] ?? "127.0.0.1";
                IPAddress ipAddress;
                if (IPAddress.TryParse(host, out var parsedIp))
                {
                    ipAddress = parsedIp;
                }
                else
                {
                    var addresses = await Dns.GetHostAddressesAsync(host);
                    ipAddress = addresses.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork) ?? addresses[0];
                }

                _serverEndPoint = new IPEndPoint(ipAddress, 9001);

                await RealtimeClient.Instance.SendPacketAsync(new
                {
                    type = "VOICE_JOIN",
                    channelId = (ulong)_currentChannelId,
                    udpPort = localPort
                });

                InitAudioPlayback();
                _ = Task.Run(() => ReceiveLoopAsync(_cts.Token));
                StartRecording();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VoiceClient] JoinVoiceChannelAsync error: {ex.Message}");
                await LeaveVoiceChannelAsync();
            }
        }

        public async Task LeaveVoiceChannelAsync()
        {
            if (_currentChannelId == 0) return;

            try
            {
                if (RealtimeClient.Instance.IsConnected)
                {
                    await RealtimeClient.Instance.SendPacketAsync(new
                    {
                        type = "VOICE_LEAVE"
                    });
                }
            }
            catch { }

            _currentChannelId = 0;
            _cts?.Cancel();

            if (_waveIn != null)
            {
                _waveIn.DataAvailable -= OnAudioDataAvailable;
                try { _waveIn.StopRecording(); } catch { }
                _waveIn.Dispose();
                _waveIn = null;
            }

            if (_udpClient != null)
            {
                _udpClient.Close();
                _udpClient = null;
            }

            lock (_userStreams)
            {
                foreach (var stream in _userStreams.Values)
                {
                    _mixer?.RemoveMixerInput(stream.SampleProvider);
                }
                _userStreams.Clear();
            }

            _encoder = null;
            _sequenceNum = 0;
            _timestamp = 0;

            lock (_pcmAccumulator)
            {
                _pcmAccumulator.Clear();
            }
        }

        private void InitAudioPlayback()
        {
            if (_waveOut != null) return;

            // 믹서는 Float 포맷 사용 (48kHz, Mono)
            var format = WaveFormat.CreateIeeeFloatWaveFormat(SAMPLE_RATE, CHANNELS);
            _mixer = new MixingSampleProvider(format)
            {
                ReadFully = true
            };

            _waveOut = new WaveOutEvent { DesiredLatency = 40 };
            _waveOut.Init(_mixer);
            _waveOut.Play();
        }

        private void StartRecording()
        {
            _encoder = OpusCodecFactory.CreateEncoder(SAMPLE_RATE, CHANNELS, OpusApplication.OPUS_APPLICATION_VOIP);
            _encoder.Bitrate = 32000;

            _waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(SAMPLE_RATE, BITS_PER_SAMPLE, CHANNELS),
                BufferMilliseconds = FRAME_MS
            };
            _waveIn.DataAvailable += OnAudioDataAvailable;
            _waveIn.StartRecording();
        }

        private void OnAudioDataAvailable(object? sender, WaveInEventArgs e)
        {
            if (IsMuted || _udpClient == null || _serverEndPoint == null || RealtimeClient.Instance.SessionToken == null)
            {
                lock (_pcmAccumulator) { _pcmAccumulator.Clear(); }
                return;
            }

            lock (_pcmAccumulator)
            {
                for (int i = 0; i < e.BytesRecorded; i++)
                {
                    _pcmAccumulator.Add(e.Buffer[i]);
                }

                while (_pcmAccumulator.Count >= FRAME_BYTES)
                {
                    byte[] pcmFrame = new byte[FRAME_BYTES];
                    _pcmAccumulator.CopyTo(0, pcmFrame, 0, FRAME_BYTES);
                    _pcmAccumulator.RemoveRange(0, FRAME_BYTES);

                    SendVoiceFrame(pcmFrame);
                }
            }
        }

        private void SendVoiceFrame(byte[] pcmFrame)
        {
            if (_encoder == null || _udpClient == null || _serverEndPoint == null) return;

            short[] pcmSamples = new short[pcmFrame.Length / 2];
            Buffer.BlockCopy(pcmFrame, 0, pcmSamples, 0, pcmFrame.Length);

            byte[] opusBuffer = new byte[MAX_OPUS_BUFFER];

            int encodedLength = _encoder.Encode(pcmSamples.AsSpan(0, FRAME_SAMPLES), FRAME_SAMPLES, opusBuffer.AsSpan(), opusBuffer.Length);

            if (encodedLength <= 0) return;

            byte[] packet = new byte[UDP_HEADER_BYTES + encodedLength];

            byte[] token = RealtimeClient.Instance.SessionToken!;
            Array.Copy(token, 0, packet, 0, Math.Min(token.Length, 32));

            byte[] channelIdBytes = BitConverter.GetBytes((ulong)_currentChannelId);
            Array.Copy(channelIdBytes, 0, packet, 32, 8);

            byte[] seqBytes = BitConverter.GetBytes(_sequenceNum++);
            Array.Copy(seqBytes, 0, packet, 40, 4);

            byte[] tsBytes = BitConverter.GetBytes(_timestamp);
            _timestamp += FRAME_SAMPLES;
            Array.Copy(tsBytes, 0, packet, 44, 4);

            Array.Copy(opusBuffer, 0, packet, UDP_HEADER_BYTES, encodedLength);

            try
            {
                _udpClient.Send(packet, packet.Length, _serverEndPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VoiceClient] Send voice packet failed: {ex.Message}");
            }
        }

        private async Task ReceiveLoopAsync(CancellationToken ct)
        {
            // 디코더 출력 및 재생 버퍼용 포맷 (48kHz, 16bit, Mono)
            var waveFormat = new WaveFormat(SAMPLE_RATE, BITS_PER_SAMPLE, CHANNELS);

            while (!ct.IsCancellationRequested && _udpClient != null)
            {
                try
                {
                    var result = await _udpClient.ReceiveAsync(ct);
                    byte[] data = result.Buffer;

                    if (data.Length < UDP_HEADER_BYTES) continue;
                    if (IsDeafened) continue;

                    long senderUserId = BitConverter.ToInt64(data, 0);
                    ulong channelId = BitConverter.ToUInt64(data, 32);
                    if (channelId != (ulong)_currentChannelId) continue;

                    int opusLength = data.Length - UDP_HEADER_BYTES;
                    if (opusLength <= 0) continue;
                    
                    UserVoiceStream stream;
                    lock (_userStreams)
                    {
                        if (!_userStreams.TryGetValue(senderUserId, out stream!))
                        {
                            stream = new UserVoiceStream(waveFormat);
                            _userStreams[senderUserId] = stream;

                            InitAudioPlayback();
                            _mixer?.AddMixerInput(stream.SampleProvider);
                        }
                    }

                    byte[] opusPayload = new byte[opusLength];
                    Array.Copy(data, UDP_HEADER_BYTES, opusPayload, 0, opusLength);

                    short[] decodedPcm = new short[FRAME_SAMPLES];

                    int decodedSamples = stream.Decoder.Decode(opusPayload.AsSpan(), decodedPcm.AsSpan(0, FRAME_SAMPLES), FRAME_SAMPLES);
                    
                    if (decodedSamples > 0)
                    {
                        byte[] pcmBytes = new byte[decodedSamples * 2];
                        Buffer.BlockCopy(decodedPcm, 0, pcmBytes, 0, pcmBytes.Length);

                        stream.BufferedProvider.AddSamples(pcmBytes, 0, pcmBytes.Length);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[VoiceClient] UDP Receive error: {ex.Message}");
                }
            }
        }

        public void RemoveUserStream(long userId)
        {
            lock (_userStreams)
            {
                if (_userStreams.TryGetValue(userId, out var stream))
                {
                    _userStreams.Remove(userId);
                    _mixer?.RemoveMixerInput(stream.SampleProvider);
                }
            }
        }

        public void Shutdown()
        {
            _cts?.Cancel();

            if (_waveIn != null)
            {
                try { _waveIn.StopRecording(); } catch { }
                _waveIn.Dispose();
                _waveIn = null;
            }

            if (_waveOut != null)
            {
                try { _waveOut.Stop(); } catch { }
                _waveOut.Dispose();
                _waveOut = null;
            }

            _udpClient?.Close();
        }

        private class UserVoiceStream
        {
            public IOpusDecoder Decoder { get; }
            public BufferedWaveProvider BufferedProvider { get; }
            public ISampleProvider SampleProvider { get; }

            public UserVoiceStream(WaveFormat format)
            {
                Decoder = OpusCodecFactory.CreateDecoder(SAMPLE_RATE, CHANNELS);
                BufferedProvider = new BufferedWaveProvider(format)
                {
                    DiscardOnBufferOverflow = true
                };
                SampleProvider = BufferedProvider.ToSampleProvider();
            }
        }
    }
}