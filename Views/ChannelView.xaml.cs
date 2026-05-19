using isegoria_wpf.Models;
using isegoria_wpf.Services;
using System;
using System.Collections.Generic;
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

            this.Loaded += async (s, e) => await LoadMembersAsync();

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

        private void SendMessage()
        {
            //@TODO 텍스트 전송 로직
            string text = MessageInput.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            // 메시지 아이템 생성
            var item = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 12)
            };

            // 프로필 이미지
            var ellipse = new Ellipse { Width = 36, Height = 36, Margin = new Thickness(0, 0, 10, 0) };
            ellipse.Fill = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/default_profile.png")));

            // 텍스트 영역
            var textPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

            var namePanel = new StackPanel { Orientation = Orientation.Horizontal };
            namePanel.Children.Add(new TextBlock
            {
                Text = "사용자명",
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
                Text = text,
                Foreground = Brushes.White,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap
            };

            textPanel.Children.Add(namePanel);
            textPanel.Children.Add(messageText);

            item.Children.Add(ellipse);
            item.Children.Add(textPanel);

            MessageList.Children.Add(item);
            MessageInput.Clear();

            // 스크롤 맨 아래로
            var scrollViewer = GetScrollViewer(MessageList);
            scrollViewer?.ScrollToEnd();
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
            var members = await ApiServer.GetServerMembersAsync(_serverId);
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
    }
}
