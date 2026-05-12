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
                    var serverButton = new Button
                    {
                        Style = (Style)FindResource("ServerButtonStyle"),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 8)
                    };

                    serverButton.Click += (s, e) =>
                    {
                        mainWindow.MainContent.Content = new Views.ChannelView();
                    };

                    serverButton.Content = new Image
                    {
                        Source = new BitmapImage(new Uri("/Assets/default_profile.png", UriKind.Relative)),
                        Width = 35,
                        Height = 35
                    };

                    int addButtonIndex = mainWindow.ServerList.Children.IndexOf(mainWindow.AddServerButton);
                    mainWindow.ServerList.Children.Insert(addButtonIndex, serverButton);

                    mainWindow.UpdateMainContent();
                }
            }
        }

        private void JoinServerButton_Click(object sender, RoutedEventArgs e)
        {
            var modal = new JoinServerModal();
            modal.Owner = Window.GetWindow(this);

            modal.ShowDialog();
        }

    }
}
