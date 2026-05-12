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
    public partial class FindPasswordView : UserControl
    {
        public FindPasswordView()
        {
            InitializeComponent();
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var parent = Window.GetWindow(this) as LoginWindow;
            parent?.NavigateTo(new LoginView());
        }

    }
}
