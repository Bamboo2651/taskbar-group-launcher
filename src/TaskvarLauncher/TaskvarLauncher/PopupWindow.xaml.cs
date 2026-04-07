using System.Diagnostics;
using System.Windows;
using TaskbarLauncher.Models;
using System.Windows.Controls;

namespace TaskbarLauncher
{
    public partial class PopupWindow : Window
    {
        public PopupWindow(string groupId)
        {
            InitializeComponent();

            var configManager = new ConfigManager();
            var groups = configManager.LoadGroups();
            var group = groups.Find(g => g.Id == groupId);

            if (group == null)
            {
                MessageBox.Show(
                    "グループが見つかりませんでした。",
                    "エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Close();
                return;
            }

            GroupTitle.Text = group.Name;
            AppItemsControl.ItemsSource = group.Apps;

            PositionWindow();
        }

        private void PositionWindow()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            UpdateLayout();

            Left = (screenWidth / 2) - (ActualWidth / 2);
            Top = screenHeight - ActualHeight - 48;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Close();
        }

        private void AppButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string path)
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                Close();
            }
        }
    }
}