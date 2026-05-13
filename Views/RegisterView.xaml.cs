using isegoria_wpf.ViewModels;
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
    /// RegisterView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RegisterView : UserControl
    {
        public RegisterView()
        {
            InitializeComponent();
            
            if(DataContext is AuthViewModel vm)
            {
                vm.OnRegisterSuccess = () =>
                {
                    var parent = Window.GetWindow(this) as LoginWindow;
                    parent?.NavigateTo(new LoginView());
                };
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if(DataContext is AuthViewModel vm)
            {
                vm.RegisterPassword = PasswordInput.Password;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var parent = Window.GetWindow(this) as LoginWindow;
            parent?.NavigateTo(new LoginView());
        }
    }
}
