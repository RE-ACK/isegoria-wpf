using isegoria_wpf.Models;
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
using System.Windows.Shapes;

namespace isegoria_wpf.Views.Modals
{
    /// <summary>
    /// ProfileModal.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ProfileModal : Window
    {
        public ProfileModal()
        {
            InitializeComponent();

            this.MouseLeftButtonDown += (s, e) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                    this.DragMove();
            };

            LoadUserInfo();
        }

        private bool _isEditing = false;


        private void LoadUserInfo()
        {
            var user = User.CurrentUser;
            if (user == null) return;

            UsernameInput.Text = user.Username;
            UsernameText.Text = user.Username;

            UserTagText.Text = $"#{user.Id:D4}";

            if (!string.IsNullOrEmpty(user.AvatarUrl))
            {
                var btn = ProfileImageButton;
                btn.ApplyTemplate();
                if (btn.Template.FindName("ProfileImageBrush", btn) is ImageBrush brush)
                    brush.ImageSource = new BitmapImage(new Uri(user.AvatarUrl));
            }
        }

        //====================================================================================//

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            _isEditing = !_isEditing;
            UsernameInput.Visibility = _isEditing ? Visibility.Visible : Visibility.Collapsed;
            UsernameText.Visibility = _isEditing ? Visibility.Collapsed : Visibility.Visible;

            if (_isEditing)
                UsernameInput.Focus();
        }

        private void ProfileImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "프로필 이미지 선택",
                Filter = "이미지 파일|*.png;*.jpg;*.jpeg;*.gif;*.bmp",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                var image = new BitmapImage(new Uri(dialog.FileName));
                var btn = sender as Button;
                btn?.ApplyTemplate();
                if (btn?.Template.FindName("ProfileImageBrush", btn) is ImageBrush brush)
                    brush.ImageSource = image;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
