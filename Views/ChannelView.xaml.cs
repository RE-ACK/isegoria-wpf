using isegoria_wpf.Models;
using isegoria_wpf.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

            //this.Loaded += async (s, e) => await LoadMembersAsync();
            this.Loaded += (s, e) =>
            {
                _ = LoadMembersAsync();
                RealtimeClient.Instance.OnPacketReceived += OnRealtimePacketReceived;
            };
            this.Unloaded += (s, e) =>
            {
                RealtimeClient.Instance.OnPacketReceived -= OnRealtimePacketReceived;
            };

            MessageInput.KeyDown += MessageInput_KeyDown;
        }

        private bool _isVoiceMuted = false;
        private bool _isMicMuted = false;

        private long _serverId;

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
            var saveResult = await ApiClient.CreateMessageAsync(6, text);

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
            _isVoiceMuted = !_isVoiceMuted;
            if (sender is Button btn)
            {
                btn.ApplyTemplate();
                if (btn.Template.FindName("VoiceIcon", btn) is Image img)
                {
                    img.Source = new BitmapImage(new Uri(
                        _isVoiceMuted ? "/Assets/voice_mute_icon.png" : "/Assets/voice_icon.png",
                        UriKind.Relative));
                }
            }
        }

        private void MicToggle_Click(object sender, RoutedEventArgs e)
        {
            _isMicMuted = !_isMicMuted;
            if (sender is Button btn)
            {
                btn.ApplyTemplate();
                if (btn.Template.FindName("MicIcon", btn) is Image img)
                {
                    img.Source = new BitmapImage(new Uri(
                        _isMicMuted ? "/Assets/mic_mute_icon.png" : "/Assets/mic_icon.png",
                        UriKind.Relative));
                }
            }
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            // 나중에 모달 연결
        }

        private async void TextChannel_Click(object sender, RoutedEventArgs e)
        {
            long channelId = 6;

            await RealtimeClient.Instance.SendPacketAsync(new
            {
                type = "JOIN_TEXT",
                channelId = channelId
            });
        }

        private void VoiceChannel_Click(object sender, RoutedEventArgs e)
        {
            VoiceParticipants.Visibility = Visibility.Visible;
            VoiceStatusBar.Visibility = Visibility.Visible;
            VoiceChannelNameText.Text = "음성채널1 / 서버이름";
        }

        private void LeaveVoice_Click(object sender, RoutedEventArgs e)
        {
            VoiceParticipants.Visibility = Visibility.Collapsed;
            VoiceStatusBar.Visibility = Visibility.Collapsed;
        }

        //임시로 오프라인에 유저목록 몰아넣음

        public async Task LoadMembersAsync()
        {
            var members = await ApiClient.GetServerMembersAsync(_serverId);
            if (members == null) return;

            OnlineMemberList.Children.Clear();
            OfflineMemberList.Children.Clear();

            var onlineList = members.Where(m => m.IsOnline).ToList();
            var offlineList = members.Where(m => !m.IsOnline).ToList();

            OnlineCountText.Text = $"온라인 - {onlineList.Count}";
            OfflineCountText.Text = $"오프라인 - {offlineList.Count}";

            foreach (var member in onlineList)
            {
                var btn = new Views.Buttons.MemberButton
                {
                    Username = member.Username ?? $"유저 {member.UserId}",
                    AvatarUrl = member.AvatarUrl?? "pack://application:,,,/Assets/default_profile.png",
                    IsOnline = true
                };
                OnlineMemberList.Children.Add(btn);
            }

            foreach (var member in offlineList)
            {
                var btn = new Views.Buttons.MemberButton
                {
                    Username = member.Username ?? $"유저 {member.UserId}",
                    AvatarUrl = member.AvatarUrl ?? "pack://application:,,,/Assets/default_profile.png",
                    IsOnline = false
                };
                OfflineMemberList.Children.Add(btn);
            }
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
                    // TODO: 임시로 6번 채널 고정이 아닌, 현재 입장 중인 채널ID와 일치하는지 확인
                    // if (channelId == _currentTextChannelId)
                    if (channelId == 6)
                    {
                        // 화면에 받은 메시지 렌더링
                        AddMessageToUI(senderName, senderAvatarUrl, content);
                    }
                }
            });
        }

        private void AddMessageToUI(string senderName, string senderAvatarurl, string content)
        {
            var item = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 12)
            };

            // 프로필 이미지
            var ellipse = new Ellipse { Width = 36, Height = 36, Margin = new Thickness(0, 0, 10, 0) };
            ellipse.Fill = new ImageBrush(new BitmapImage(new Uri(!string.IsNullOrEmpty(senderAvatarurl) ? senderAvatarurl : "pack://application:,,,/Assets/default_profile.png")));
            //ellipse.Fill = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/default_profile.png")));

            // 텍스트 영역
            var textPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

            var namePanel = new StackPanel { Orientation = Orientation.Horizontal };
            namePanel.Children.Add(new TextBlock
            {
                Text = senderName,
                Foreground = Brushes.White,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 8, 0)
            });
            namePanel.Children.Add(new TextBlock
            {
                Text = DateTime.Now.ToString("tt h:mm"),
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

            MessageList.Children.Add(item);

            // 스크롤 맨 아래로
            var scrollViewer = GetScrollViewer(MessageList);
            scrollViewer?.ScrollToEnd();
        }
    }
}
