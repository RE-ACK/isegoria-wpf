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

namespace isegoria_wpf.Views.Buttons
{
    /// <summary>
    /// MemberButton.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MemberButton : UserControl
    {
        public static readonly DependencyProperty UsernameProperty =
            DependencyProperty.Register("Username", typeof(string), typeof(MemberButton));

        public static readonly DependencyProperty AvatarUrlProperty =
            DependencyProperty.Register("AvatarUrl", typeof(string), typeof(MemberButton));

        public static readonly DependencyProperty IsOnlineProperty =
            DependencyProperty.Register("IsOnline", typeof(bool), typeof(MemberButton),
                new PropertyMetadata(false, OnIsOnlineChanged));

        public string Username
        {
            get => (string)GetValue(UsernameProperty);
            set => SetValue(UsernameProperty, value);
        }

        public string? AvatarUrl
        {
            get => (string?)GetValue(AvatarUrlProperty);
            set => SetValue(AvatarUrlProperty, value);
        }

        public bool IsOnline
        {
            get => (bool)GetValue(IsOnlineProperty);
            set => SetValue(IsOnlineProperty, value);
        }

        // 상태에 따라 색상 자동 변경
        public Brush StatusColor => IsOnline
            ? new SolidColorBrush(Color.FromRgb(0x2D, 0xB7, 0x7F))  // 초록
            : new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66));  // 회색

        public Brush TextColor => IsOnline
            ? new SolidColorBrush(Colors.White)
            : new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));

        private static void OnIsOnlineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var btn = (MemberButton)d;
            btn.OnPropertyChanged(nameof(StatusColor));
            btn.OnPropertyChanged(nameof(TextColor));
        }

        private void OnPropertyChanged(string name)
        {
            // DependencyProperty 변경 알림
        }

        public MemberButton()
        {
            InitializeComponent();
        }
    }
}
