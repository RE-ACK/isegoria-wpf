using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;


namespace isegoria_wpf.Services
{
    // TCP
    public class RealtimeClient
    {
        private static readonly RealtimeClient _instance = new RealtimeClient();
        public static RealtimeClient Instance => _instance;

        private TcpClient? _tcpClient;
        private NetworkStream? _stream;
        private CancellationTokenSource _cts;

        // 패킷 수신 이벤트
        public event Action<string, JsonElement>? OnPacketReceived;

        private RealtimeClient() { }

        public async Task ConnectAsync(string host, int port)
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(host, port);
            _stream = _tcpClient.GetStream();

            _cts = new CancellationTokenSource();
            _ = Task.Run(() => ReceiveLoopAsync(_cts.Token));
        }

        public async Task SendPacketAsync(object packet)
        {
            if (_stream == null) return;

            string json = JsonSerializer.Serialize(packet);
            byte[] body = Encoding.UTF8.GetBytes(json);

            // Length-Prefix: 빅엔디안 4바이트
            byte[] lengthBytes = BitConverter.GetBytes((uint)body.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthBytes);

            byte[] buffer = new byte[4 + body.Length];
            Array.Copy(lengthBytes, 0, buffer, 0, 4);
            Array.Copy(body, 0, buffer, 4, body.Length);

            await _stream.WriteAsync(buffer);
        }


        private async Task ReceiveLoopAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    // Length-Prefix 4Byte Read
                    byte[] lengthBuf = new byte[4];
                    await ReadExactAsync(lengthBuf, 4, ct);

                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(lengthBuf);
                    uint length = BitConverter.ToUInt32(lengthBuf, 0);

                    if (length == 0 || length > 4096)
                        continue;

                    // Body read
                    byte[] bodyBuf = new byte[length];
                    await ReadExactAsync(bodyBuf, (int)length, ct);

                    string json = Encoding.UTF8.GetString(bodyBuf);

                    // Parseing & Publish Event
                    var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("type", out var typeProp))
                    {
                        string type = typeProp.GetString() ?? "";
                        OnPacketReceived?.Invoke(type, doc.RootElement);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RealtimeClient] ReceiveLoop error: {ex.Message}");
                Disconnect();
            }
        }

        private async Task ReadExactAsync(byte[] buffer, int count, CancellationToken ct)
        {
            int offset = 0;
            while (offset < count)
            {
                int read = await _stream!.ReadAsync(buffer, offset, count - offset, ct);
                if (read == 0)
                    throw new Exception("CONNECTION CLOSED");
                offset += read;
            }
        }

        public void Disconnect()
        {
            _cts?.Cancel();
            _stream?.Close();
            _tcpClient?.Close();
            _stream = null;
            _tcpClient = null;
        }
        public bool IsConnected => _tcpClient?.Connected ?? false;

    }
}
