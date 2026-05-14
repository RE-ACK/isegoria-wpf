using isegoria_wpf.Views.Modals;
using System.Diagnostics;
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


        public void UpdateMainContent(string? serverName = null)
        {
            bool hasServer = ServerList.Children.Count > 1;

            if (hasServer)
            {
                MainContent.Content = new Views.ChannelView(serverName ?? string.Empty);
            }
            else
            {
                MainContent.Content = new Views.WelcomeView();
            }
        }


        // ============================================================================== //
        // Callback
        // ============================================================================== //

        private void UserIconButton_Click(object sender, RoutedEventArgs e)
        {
            var modal = new ProfileModal();
            modal.Owner = Window.GetWindow(this);

            modal.ShowDialog();
        }
        private void AddServerButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new Views.WelcomeView();
        }

        public void AddServerToList(string? iconUrl = null, string? serverName = null)
        {
            Debug.WriteLine($"=== AddServerToList 호출 ===");
            Debug.WriteLine($"serverName: {serverName}");
            Debug.WriteLine($"iconUrl: {iconUrl ?? "null"}");



            var serverButton = new Views.Buttons.ServerButton
            {
                IconUrl = iconUrl ?? "/Assets/default_profile.png",
                ServerName = serverName ?? string.Empty
            };


            Debug.WriteLine($"ServerButton.IconUrl 설정값: {serverButton.IconUrl}");
            serverButton.ServerClicked += (s, e) =>
            {
                MainContent.Content = new Views.ChannelView(serverName ?? string.Empty);
            };

            int addButtonIndex = ServerList.Children.IndexOf(AddServerButton);
            ServerList.Children.Insert(addButtonIndex, serverButton);

            UpdateMainContent(serverName ?? string.Empty);
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