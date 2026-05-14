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

    public partial class ServerButton : UserControl
    {
        public static readonly DependencyProperty IconUrlProperty =
            DependencyProperty.Register("IconUrl", typeof(string), typeof(ServerButton));

        public static readonly DependencyProperty ServerIdProperty =
            DependencyProperty.Register("ServerId", typeof(long), typeof(ServerButton));

        public static readonly DependencyProperty ServerNameProperty =
            DependencyProperty.Register("ServerName", typeof(string), typeof(ServerButton));    

        public string IconUrl
        {
            get => (string)GetValue(IconUrlProperty);
            set => SetValue(IconUrlProperty, value);
        }

        public long ServerId
        {
            get => (long)GetValue(ServerIdProperty);
            set => SetValue(ServerIdProperty, value);
        }

        public string ServerName
        {
            get => (string)GetValue(ServerNameProperty);
            set => SetValue(ServerNameProperty, value);
        }

        public event RoutedEventHandler? ServerClicked;

        public ServerButton()
        {
            InitializeComponent();
        }

        private void ServerButton_Click(object sender, RoutedEventArgs e)
        {
            ServerClicked?.Invoke(this, e);
        }

        private static void OnIconUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // DependencyProperty 자동 처리됨
        }
    }
}
