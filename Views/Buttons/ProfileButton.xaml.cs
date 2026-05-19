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
    public partial class ProfileButton : UserControl
    {
        public static readonly DependencyProperty AvatarUrlProperty =
    DependencyProperty.Register("AvatarUrl", typeof(string), typeof(ProfileButton),
        new PropertyMetadata("/Assets/default_profile.png"));

        public event RoutedEventHandler? ProfileClicked;
        public string? AvatarUrl
        {
            get => (string?)GetValue(AvatarUrlProperty);
            set => SetValue(AvatarUrlProperty, value);
        }

        public ProfileButton()
        {
            InitializeComponent();
        }
        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileClicked?.Invoke(this, e);
        }

    }
}
