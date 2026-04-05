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
    }
}