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
    /// channelButton.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChannelButton : UserControl
    {
        public static readonly DependencyProperty ChannelNameProperty =
            DependencyProperty.Register("ChannelName", typeof(string), typeof(ChannelButton));

        public static readonly DependencyProperty ChannelIdProperty =
            DependencyProperty.Register("ChannelId", typeof(long), typeof(ChannelButton));

        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register("IconSource", typeof(string), typeof(ChannelButton),
                new PropertyMetadata("/Assets/chat_icon.png"));

        public string ChannelName
        {
            get => (string)GetValue(ChannelNameProperty);
            set => SetValue(ChannelNameProperty, value);
        }

        public long ChannelId
        {
            get => (long)GetValue(ChannelIdProperty);
            set => SetValue(ChannelIdProperty, value);
        }

        public string IconSource
        {
            get => (string)GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        public event RoutedEventHandler? ChannelClicked;

        public ChannelButton()
        {
            InitializeComponent();
        }

        private void ChannelButton_Click(object sender, RoutedEventArgs e)
        {
            ChannelClicked?.Invoke(this, e);
        }
    }
}
