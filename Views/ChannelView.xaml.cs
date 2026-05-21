using isegoria_wpf.Models;
using isegoria_wpf.Models.Dtos;
using isegoria_wpf.Services;
using isegoria_wpf.Views.Buttons;
using isegoria_wpf.Views.Modals;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static isegoria_wpf.Models.Dtos.channelDto;
using static isegoria_wpf.Models.Dtos.ServerDto;
using static System.Net.WebRequestMethods;

namespace isegoria_wpf.Views
{
    /// <summary>
    /// ChannelView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChannelView : UserControl
    {
        public ChannelView(string? servername = null, long serverId = 0)
        {
            InitializeComponent();
            ServerNameText?.Text = servername;
            _serverId = serverId;

            UsernameText.Text = User.CurrentUser?.Username ?? "사용자명";
            UserTagText.Text = $"#{User.CurrentUser?.Id:D4}";

            this.Loaded += (s, e) =>
            {
                _ = LoadMembersAsync();
                _ = LoadChannelsAsync();
                RealtimeClient.Instance.OnPacketReceived += OnRealtimePacketReceived;
            };
            this.Unloaded += (s, e) =>
            {
                RealtimeClient.Instance.OnPacketReceived -= OnRealtimePacketReceived;

                if (_currentTextChannelId != 0)
                {
                    _ = RealtimeClient.Instance.SendPacketAsync(new { type = "LEAVE_TEXT" });
                    _currentTextChannelId = 0;
                }

                if (_currentVoiceChannelId != 0)
                {
                    _ = VoiceClient.Instance.LeaveVoiceChannelAsync();
                    _currentVoiceChannelId = 0;
                }

                VoiceParticipants.Children.Clear();
                VoiceParticipants.Visibility = Visibility.Collapsed;
                VoiceStatusBar.Visibility = Visibility.Collapsed;
                _members = null;
            };

            ChatScrollViewer.ScrollChanged += ChatScrollViewer_ScrollChanged;
            MessageInput.KeyDown += MessageInput_KeyDown;
        }


        //===================================================================================//

        private long _currentVoiceChannelId = 0;

        private long _serverId;
        private long _currentTextChannelId = 0;

        private List<(long id, string name)> textChannelList = [];
        private List<MemberInfo>? _members;

        private bool _isLoadingMessages = false; // 중복 요청 방지 플래그
        private long _oldestMessageId = 0;

        //===================================================================================//

        private void Add_File_Click(object sender, RoutedEventArgs e)
        {
            //@TODO 파일 첨부 로직
        }

        private void Send_Click(object sender, RoutedEventArgs e) => SendMessage();

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SendMessage();
        }

        private async void SendMessage()
        {
            string text = MessageInput.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            MessageInput.Clear();

            // API 서버에 메시지 저장 (HTTP)
            var saveResult = await ApiClient.CreateMessageAsync(_currentTextChannelId, text);

            if (saveResult)
            {
                // 성공 시 소켓을 통해 전파 (나를 포함한 채널 내 모든 사람에게 발송됨)
                await RealtimeClient.Instance.SendPacketAsync(new
                {
                    type = "CHAT_MSG",
                    serverId = _serverId,
                    userName = User.CurrentUser?.Username ?? "",
                    avatarUrl = User.CurrentUser?.AvatarUrl ?? "",
                    content = text
                });
            }
        }

        private ScrollViewer? GetScrollViewer(DependencyObject o)
        {
            if (o is ScrollViewer sv) return sv;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);
                var result = GetScrollViewer(child);
                if (result != null) return result;
            }
            return null;
        }

        private void VoiceToggle_Click(object sender, RoutedEventArgs e)
        {
            // 헤드셋(귀막기) 토글
            VoiceClient.Instance.IsDeafened = !VoiceClient.Instance.IsDeafened;
            if (sender is Button btn)
            {
                btn.ApplyTemplate();
                if (btn.Template.FindName("VoiceIcon", btn) is Image img)
                {
                    img.Source = new BitmapImage(new Uri(
                        VoiceClient.Instance.IsDeafened ? "/Assets/voice_mute_icon.png" : "/Assets/voice_icon.png",
                        UriKind.Relative));
                }
            }
        }

        private void MicToggle_Click(object sender, RoutedEventArgs e)
        {
            // 마이크 음소거 토글
            VoiceClient.Instance.IsMuted = !VoiceClient.Instance.IsMuted;
            if (sender is Button btn)
            {
                btn.ApplyTemplate();
                if (btn.Template.FindName("MicIcon", btn) is Image img)
                {
                    img.Source = new BitmapImage(new Uri(
                        VoiceClient.Instance.IsMuted ? "/Assets/mic_mute_icon.png" : "/Assets/mic_icon.png",
                        UriKind.Relative));
                }
            }
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            // 나중에 모달 연결
            var modal = new ProfileModal();
            modal.Owner = Window.GetWindow(this);

            modal.ShowDialog();
        }

        private void LeaveVoice_Click(object sender, RoutedEventArgs e)
        {
            _ = VoiceClient.Instance.LeaveVoiceChannelAsync();
            _currentVoiceChannelId = 0;
            VoiceParticipants.Visibility = Visibility.Collapsed;
            VoiceStatusBar.Visibility = Visibility.Collapsed;
            VoiceParticipants.Children.Clear();
        }

        //채널 목록 불러오기
        private async Task LoadChannelsAsync()
        {
            var channels = await ApiClient.GetChannelsAsync(_serverId);
            if (channels == null) return;

            TextChannelList.Children.Clear();
            VoiceChannelList.Children.Clear();
            textChannelList.Clear();

            ChannelInfo? firstTextChannel = null;

            foreach (var channel in channels)
            {
                if (channel.Type == "TEXT")
                {
                    var btn = await CreateChannelButtonAsync(channel);
                    TextChannelList.Children.Add(btn);
                    textChannelList.Add((channel.Id, channel.Name));

                    if (firstTextChannel == null)
                    {
                        firstTextChannel = channel;
                    }
                }
                else if (channel.Type == "VOICE")
                {
                    var btn = await CreateChannelButtonAsync(channel);
                    VoiceChannelList.Children.Add(btn);
                }
            }

            if (firstTextChannel != null)
            {
                await JoinTextChannelAsync(firstTextChannel);
            }
        }

        public async Task LoadMembersAsync()
        {
            var members = await ApiClient.GetServerMembersAsync(_serverId);
            if (members == null) return;

            _members = members;

            // 소켓 서버에 현재 서버 멤버 목록 구독 요청
            var memberIds = members.Select(m => m.UserId).ToList();
            await RealtimeClient.Instance.SendPacketAsync(new
            {
                type = "SUBSCRIBE_STATUS",
                userIds = memberIds
            });

            RenderMembers();
        }

        private void RenderMembers()
        {
            if (_members == null) return;

            OnlineMemberList.Children.Clear();
            OfflineMemberList.Children.Clear();

            // RealtimeClient.Instance.OnlineUserIds 기준 최신화
            var updatedMembers = _members.Select(m => m with
            {
                IsOnline = RealtimeClient.Instance.OnlineUserIds.Contains(m.UserId)
            }).ToList();

            var onlineList = updatedMembers.Where(m => m.IsOnline).ToList();
            var offlineList = updatedMembers.Where(m => !m.IsOnline).ToList();

            OnlineCountText.Text = $"온라인 - {onlineList.Count}";
            OfflineCountText.Text = $"오프라인 - {offlineList.Count}";

            foreach (var member in onlineList)
            {
                var btn = new Views.Buttons.MemberButton
                {
                    Username = member.Username ?? $"유저 {member.UserId}",
                    AvatarUrl = (!string.IsNullOrEmpty(member.AvatarUrl) && member.AvatarUrl != "null")
                        ? member.AvatarUrl
                        : "pack://application:,,,/Assets/default_profile.png",
                    IsOnline = true
                };
                OnlineMemberList.Children.Add(btn);
            }

            foreach (var member in offlineList)
            {
                var btn = new Views.Buttons.MemberButton
                {
                    Username = member.Username ?? $"유저 {member.UserId}",
                    AvatarUrl = (!string.IsNullOrEmpty(member.AvatarUrl) && member.AvatarUrl != "null")
                        ? member.AvatarUrl
                        : "pack://application:,,,/Assets/default_profile.png",
                    IsOnline = false
                };
                OfflineMemberList.Children.Add(btn);
            }
        }

        private async Task<ChannelButton> CreateChannelButtonAsync(ChannelInfo channel)
        {
            var btn = new Views.Buttons.ChannelButton
            {
                ChannelName = channel.Name,
                ChannelId = channel.Id,
                IconSource = channel.Type == "TEXT"
                    ? "pack://application:,,,/Assets/chat_icon.png"
                    : "pack://application:,,,/Assets/voice_icon.png"
            };

            btn.ChannelClicked += async (s, e) =>
            {
                if (channel.Type == "TEXT")
                {
                    await JoinTextChannelAsync(channel);
                }
                else if (channel.Type == "VOICE")
                {
                    await JoinVoiceChannelAsync(channel);
                }
            };

            return btn;
        }

        private async Task JoinTextChannelAsync(ChannelInfo channel)
        {
            if (_currentTextChannelId != 0 && _currentTextChannelId != channel.Id)
            {
                await RealtimeClient.Instance.SendPacketAsync(new
                {
                    type = "LEAVE_TEXT"
                });
                Debug.WriteLine($"기존 채널 퇴장 요청: {_currentTextChannelId}");
            }

            _currentTextChannelId = channel.Id;
            CurrentChannelNameText.Text = channel.Name;
            await RealtimeClient.Instance.SendPacketAsync(new
            {
                type = "JOIN_TEXT",
                channelId = _currentTextChannelId
            });
            MessageList.Children.Clear();

            await LoadChannelMessagesAsync(channel.Id);

            Debug.WriteLine($"채널 자동/수동 입장 완료: {channel.Name}");
        }

        private void OnRealtimePacketReceived(string type, System.Text.Json.JsonElement json)
        {
            // UI 스레드에서 실행 보장
            Dispatcher.Invoke(() =>
            {
                if (type == "CHAT_MSG")
                {
                    // 서버가 보낸 패킷 파싱
                    long senderId = json.GetProperty("userId").GetInt64();
                    string senderName = json.GetProperty("userName").GetString() ?? "";
                    long channelId = json.GetProperty("channelId").GetInt64();
                    string content = json.GetProperty("content").GetString() ?? "";
                    string senderAvatarUrl = json.GetProperty("avatarUrl").GetString() ?? "";

                    Debug.WriteLine(senderAvatarUrl);

                    if (channelId == _currentTextChannelId)
                    {
                        // 화면에 받은 메시지 렌더링
                        AddMessageToUI(senderName, senderAvatarUrl, content);
                    }
                }
                else if (type == "USER_STATE" || type == "SUBSCRIBE_STATUS_OK")
                {
                    // 실시간 유저 상태 변경 또는 구독 리스트 수신 시 로컬 UI 갱신 (무한루프 없음)
                    RenderMembers();
                }
                else if (type == "VOICE_STATE")
                {
                    Debug.WriteLine("왔냐");
                    if (json.TryGetProperty("userId", out var userProp) &&
                        json.TryGetProperty("channelId", out var chProp) &&
                        json.TryGetProperty("joined", out var joinProp))
                    {
                        long voiceUserId = userProp.GetInt64();
                        long voiceChannelId = (long)chProp.GetUInt64();
                        bool joined = joinProp.GetBoolean();

                        Debug.WriteLine("나여 :" ,voiceUserId);
                        UpdateVoiceParticipantsUI(voiceUserId, joined);
                    }
                }
            });
        }

        private async Task JoinVoiceChannelAsync(ChannelInfo channel)
        {
            if (_currentVoiceChannelId == channel.Id) return;

            _currentVoiceChannelId = channel.Id;

            // VoiceClient에 입장 요청
            await VoiceClient.Instance.JoinVoiceChannelAsync(channel.Id);

            // VoiceStatusBar 상단에 채널명 표시 및 상태 바 활성화
            VoiceChannelNameText.Text = $"{channel.Name}";
            VoiceParticipants.Visibility = Visibility.Visible;
            VoiceStatusBar.Visibility = Visibility.Visible;
        }

        private void UpdateVoiceParticipantsUI(long userId, bool joined)
        {
            string tag = $"voice_user_{userId}";

            if (joined)
            {
                // 이미 존재하면 추가하지 않음
                foreach (UIElement el in VoiceParticipants.Children)
                {
                    if (el is StackPanel sp && sp.Tag?.ToString() == tag) return;
                }

                // 해당 멤버 정보 조회
                var member = _members?.FirstOrDefault(m => m.UserId == userId);
                string displayName = member?.Username ?? $"유저 {userId}";
                string avatarUrl = (!string.IsNullOrEmpty(member?.AvatarUrl) && member?.AvatarUrl != "null")
                    ? member!.AvatarUrl!
                    : "pack://application:,,,/Assets/default_profile.png";

                var row = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 4, 0, 0),
                    Tag = tag
                };

                var ellipse = new Ellipse { Width = 15, Height = 15, Margin = new Thickness(0, 0, 8, 0) };
                ellipse.Fill = new ImageBrush(new BitmapImage(new Uri(avatarUrl, UriKind.RelativeOrAbsolute)));

                var nameText = new TextBlock
                {
                    Text = displayName,
                    Foreground = Brushes.White,
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center
                };

                row.Children.Add(ellipse);
                row.Children.Add(nameText);
                VoiceParticipants.Children.Add(row);
            }
            else
            {
                // 퇴장한 유저 행 제거
                UIElement? toRemove = null;
                foreach (UIElement el in VoiceParticipants.Children)
                {
                    if (el is StackPanel sp && sp.Tag?.ToString() == tag)
                    {
                        toRemove = el;
                        break;
                    }
                }
                if (toRemove != null)
                    VoiceParticipants.Children.Remove(toRemove);
            }
        }

        private void AddMessageToUI(string senderName,string senderAvatarurl,string content,string? createdAt = null,bool autoScroll = true, bool prepend = false)
        {
            var item = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var ellipse = new Ellipse
            {
                Width = 36,
                Height = 36,
                Margin = new Thickness(0, 0, 10, 0)
            };

            string avatarUri =
                (!string.IsNullOrEmpty(senderAvatarurl) && senderAvatarurl != "null")
                ? senderAvatarurl
                : "pack://application:,,,/Assets/default_profile.png";

            ellipse.Fill = new ImageBrush(
                new BitmapImage(new Uri(avatarUri, UriKind.RelativeOrAbsolute)));

            var textPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };

            var namePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            namePanel.Children.Add(new TextBlock
            {
                Text = senderName,
                Foreground = Brushes.White,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 8, 0)
            });

            string displayTime;

            if (!string.IsNullOrEmpty(createdAt)
                && DateTime.TryParse(createdAt, out DateTime parsedTime))
            {
                displayTime = parsedTime.ToString("tt h:mm");
            }
            else
            {
                displayTime = DateTime.Now.ToString("tt h:mm");
            }

            namePanel.Children.Add(new TextBlock
            {
                Text = displayTime,
                Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x78, 0xCC)),
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center
            });

            var messageText = new TextBlock
            {
                Text = content,
                Foreground = Brushes.White,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap
            };

            textPanel.Children.Add(namePanel);
            textPanel.Children.Add(messageText);

            item.Children.Add(ellipse);
            item.Children.Add(textPanel);

            if (prepend)
            {
                MessageList.Children.Insert(0, item);
            }
            else
            {
                MessageList.Children.Add(item);
            }

            if (autoScroll)
            {
                ScrollToBottom();
            }
        }

        private async Task LoadChannelMessagesAsync(long channelId, long lastMessageId = 0, int size = 50)
        {
            try
            {
                string jsonResult =
                    await ApiClient.getMessages(channelId, lastMessageId, size);

                if (string.IsNullOrEmpty(jsonResult))
                    return;

                using JsonDocument doc = JsonDocument.Parse(jsonResult);

                if (!doc.RootElement.TryGetProperty("body", out JsonElement dataElement))
                    return;

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var messages = JsonSerializer.Deserialize<List<MessageDto.MessageResponse>>(dataElement.GetRawText(),options);

                if (messages == null || messages.Count == 0)
                    return;

                messages.Reverse();

                _oldestMessageId = messages.First().id;

                bool isPaging = lastMessageId != 0;

                foreach (var msg in messages)
                {
                    AddMessageToUI(msg.senderName,msg.senderImage,msg.content,msg.createdAt,autoScroll: false,prepend: isPaging);
                }

                if (!isPaging)
                {
                    ScrollToBottom();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadChannelMessagesAsync 에러: {ex.Message}");
            }
        }

        private void ScrollToBottom()
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                ChatScrollViewer.ScrollToEnd();
            }));
        }

        private async void ChatScrollViewer_ScrollChanged(object sender,ScrollChangedEventArgs e)
        {
            if (ChatScrollViewer.VerticalOffset > 0)
                return;

            if (_isLoadingMessages)
                return;

            if (_oldestMessageId == 0)
                return;

            _isLoadingMessages = true;

            double oldHeight = ChatScrollViewer.ExtentHeight;

            await LoadChannelMessagesAsync(
                _currentTextChannelId,
                _oldestMessageId
            );

            _ = Dispatcher.BeginInvoke(() =>
            {
                ChatScrollViewer.ScrollToVerticalOffset(
                    ChatScrollViewer.ExtentHeight - oldHeight
                );
            });

            _isLoadingMessages = false;
        }
    }
}
