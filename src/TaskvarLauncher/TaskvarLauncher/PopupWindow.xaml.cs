using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TaskbarLauncher.Models;

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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double workAreaHeight = SystemParameters.WorkArea.Height;

            Left = (screenWidth / 2) - (ActualWidth / 2);
            Top = workAreaHeight - ActualHeight;
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