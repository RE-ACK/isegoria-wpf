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
using System.Windows.Shapes;

namespace isegoria_wpf.Views
{
    /// <summary>
    /// LoginWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            this.MouseLeftButtonDown += (s, e) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                    this.DragMove();
            };
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
            => this.Close();

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameInput.Text;
            string password = PasswordInput.Password;

            //@TODO 로그인 로직
            Debug.WriteLine("username : " + username + "\npassword : " + password);

            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void FindId_Click(object sender, RoutedEventArgs e)
        {
            //@TODO: 아이디 찾기 창 열기

        }

        private void FindPassword_Click(object sender, RoutedEventArgs e)
        {
                 //@TODO: 아이디 찾기 창 열기
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
