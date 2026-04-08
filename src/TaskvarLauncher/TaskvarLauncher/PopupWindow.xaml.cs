using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TaskbarLauncher.Models;
using MessageBox = System.Windows.MessageBox;
using Button = System.Windows.Controls.Button;

namespace TaskbarLauncher
{
    public partial class PopupWindow : Window
    {
        // キャッシュ済みグループを受け取るコンストラクタ（新）
        public PopupWindow(string groupId, List<GroupConfig> groups)
        {
            InitializeComponent();

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

        private bool _isReady = false;
        private bool _isClosed = false;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double workAreaHeight = SystemParameters.WorkArea.Height;

            Left = (screenWidth / 2) - (ActualWidth / 2);
            Top = workAreaHeight - ActualHeight;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                _isReady = true;
            }), System.Windows.Threading.DispatcherPriority.Input);
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (_isReady && !_isClosed)
            {
                _isClosed = true;
                Close();
            }
        }

        private void AppButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string path)
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                _isClosed = true;
                Close();
            }
        }
    }
}