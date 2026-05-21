using isegoria_wpf.ViewModels;
using isegoria_wpf.Views.Buttons;
using isegoria_wpf.Views.Modals;
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
    public partial class WelcomeView : UserControl
    {
        public WelcomeView()
        {
            InitializeComponent();
        }

        private void CreateServerButton_Click(object sender, RoutedEventArgs e)
        {
            var modal = new CreateServerModal();
            modal.Owner = Window.GetWindow(this);

            if(modal.ShowDialog() == true)
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    var vm = modal.DataContext as ServerViewModel;
                    AddServerButton(vm?.SelectedServer?.IconUrl,vm?.SelectedServer?.Name, vm?.SelectedServer?.Id ?? 0);
                }
            }
        }

        private void JoinServerButton_Click(object sender, RoutedEventArgs e)
        {
            var modal = new JoinServerModal();
            modal.Owner = Window.GetWindow(this);

            if (modal.ShowDialog() == true)
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    var vm = modal.DataContext as ServerViewModel;
                    AddServerButton(
                        vm?.SelectedServer?.IconUrl,
                        vm?.SelectedServer?.Name,
                        vm?.SelectedServer?.Id ?? 0
                    );
                }
            }
        }

        private void AddServerButton(string? iconUrl = null, string? serverName = null, long serverId = 0)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.AddServerToList(iconUrl, serverName, serverId);
        }

    }
}
