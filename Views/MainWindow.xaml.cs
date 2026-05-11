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


namespace isegoria_wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            UpdateMainContent();
            //MainContent.Content = new ChatArea();
        }


        private void UpdateMainContent()
        {
            bool hasServer = ServerList.Children.Count > 1;

            if (hasServer)
            {
                MainContent.Content = new Views.ChannelView();
            }
            else
            {
                MainContent.Content = new Views.WelcomeView();
            }
        }


        // ============================================================================== //
        // Callback
        // ============================================================================== //
        private void AddServerButton_Click(object sender, RoutedEventArgs e)
        {
            //MainContent.Content = new Views.WelcomeView();

            var serverButton = new Button
            {
                Style = (Style)FindResource("ServerButtonStyle"),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            };

            serverButton.Content = new Image
            {
                Source = new BitmapImage(new Uri("/Assets/default_profile.png", UriKind.Relative)),
                Width = 35,
                Height = 35
            };

            int addButtonIndex = ServerList.Children.IndexOf(AddServerButton);
            ServerList.Children.Insert(addButtonIndex, serverButton);

            UpdateMainContent();

        }

        private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }


        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
            => this.Close();
    }
}