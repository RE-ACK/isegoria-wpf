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
using System.Windows.Shapes;

namespace isegoria_wpf.Views.Modals
{
    public partial class CreateServerModal : Window
    {
        public CreateServerModal()
        {
            InitializeComponent();

            if (DataContext is ServerViewModel vm)
            {
                vm.OnCreateSuccess = () =>
                {
                    this.DialogResult = true;
                    this.Close();
                };
            }

            this.MouseLeftButtonDown += (s, e) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                    this.DragMove();
            };
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();

    }
}
