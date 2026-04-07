using System.Collections.ObjectModel;
using System.Windows;
using TaskbarLauncher.Models;

namespace TaskbarLauncher
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<GroupConfig> Groups { get; set; } = new();
        private ConfigManager _configManager = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadGroups();
        }

        private void LoadGroups()
        {
            var groups = _configManager.LoadGroups();
            Groups.Clear();
            foreach (var group in groups)
                Groups.Add(group);
        }

        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog("グループ名を入力してください");
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                var newGroup = new GroupConfig { Name = dialog.Answer };
                Groups.Add(newGroup);
                _configManager.SaveGroups(new List<GroupConfig>(Groups));
            }
        }
        private void DeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            if (GroupList.SelectedItem is GroupConfig selected)
            {
                if (selected.Apps.Count > 0)
                {
                    MessageBox.Show(
                        "グループにアプリが残っています。\n先にアプリをすべて削除してください。",
                        "削除できません",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                Groups.Remove(selected);
                GroupTitle.Text = "グループを選択してください";
                AppList.ItemsSource = null;
                AddAppButton.IsEnabled = false;
                _configManager.SaveGroups(new List<GroupConfig>(Groups));
            }
        }

        private void RemoveApp_Click(object sender, RoutedEventArgs e)
        {
            if (AppList.SelectedItem is AppConfig selectedApp &&
                GroupList.SelectedItem is GroupConfig selectedGroup)
            {
                selectedGroup.Apps.Remove(selectedApp);
                AppList.ItemsSource = null;
                AppList.ItemsSource = selectedGroup.Apps;
                _configManager.SaveGroups(new List<GroupConfig>(Groups));
            }
        }

        private void MoveApp_Click(object sender, RoutedEventArgs e)
        {
            if (AppList.SelectedItem is AppConfig selectedApp &&
                GroupList.SelectedItem is GroupConfig currentGroup)
            {
                var otherGroups = Groups.Where(g => g != currentGroup).ToList();

                if (otherGroups.Count == 0)
                {
                    MessageBox.Show(
                        "移動先のグループがありません。\n先に別のグループを作成してください。",
                        "移動できません",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var dialog = new MoveAppDialog(otherGroups);
                dialog.Owner = this;
                if (dialog.ShowDialog() == true && dialog.SelectedGroup != null)
                {
                    currentGroup.Apps.Remove(selectedApp);
                    dialog.SelectedGroup.Apps.Add(selectedApp);
                    AppList.ItemsSource = null;
                    AppList.ItemsSource = currentGroup.Apps;
                    _configManager.SaveGroups(new List<GroupConfig>(Groups));
                }
            }
        }

        private void CreateShortcut_Click(object sender, RoutedEventArgs e)
        {
            if (GroupList.SelectedItem is GroupConfig selected)
            {
                _configManager.CreateShortcut(selected);
                MessageBox.Show(
                    $"デスクトップに「{selected.Name}.lnk」を作成しました。\nそのファイルを純正タスクバーにドラッグしてピン留めしてください。",
                    "ショートカット作成完了",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void GroupList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (GroupList.SelectedItem is GroupConfig selected)
            {
                GroupTitle.Text = selected.Name;
                AppList.ItemsSource = selected.Apps;
                AddAppButton.IsEnabled = true;
                CreateShortcutButton.IsEnabled = true;
            }
        }

        private void AddApp_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "アプリを選択",
                Filter = "実行ファイル (*.exe)|*.exe",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                if (GroupList.SelectedItem is GroupConfig selected)
                {
                    var appName = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
                    selected.Apps.Add(new AppConfig { Name = appName, Path = dialog.FileName });
                    AppList.ItemsSource = null;
                    AppList.ItemsSource = selected.Apps;
                    _configManager.SaveGroups(new List<GroupConfig>(Groups));
                }
            }
        }
    }
}