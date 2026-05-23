using isegoria_wpf.Services;
using System;
using System.Windows;
using System.Windows.Input;

namespace isegoria_wpf.Views.Modals
{
    /// <summary>
    /// Invitemodal.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InviteModal : Window
    {
        public InviteModal()
        {
            InitializeComponent();
        }

        private readonly long _serverId;

        public InviteModal(long serverId)
        {
            InitializeComponent();
            _serverId = serverId;

            this.MouseLeftButtonDown += (s, e) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                    this.DragMove();
            };

            _ = LoadInviteCodeAsync();
        }

        private async Task LoadInviteCodeAsync()
        {
            var code = await ApiClient.RegenerateInviteCodeAsync(_serverId);
            InviteCodeText.Text = code ?? "불러오기 실패";
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(InviteCodeText.Text)) return;

            Clipboard.SetText(InviteCodeText.Text);

            // 복사 완료 피드백
            CopyButtonText.Text = "완료";
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            timer.Tick += (s, _) =>
            {
                CopyButtonText.Text = "복사";
                timer.Stop();
            };
            timer.Start();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
