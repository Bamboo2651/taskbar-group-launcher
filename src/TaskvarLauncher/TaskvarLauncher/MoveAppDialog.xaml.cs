using System.Collections.Generic;
using System.Windows;
using TaskbarLauncher.Models;

namespace TaskbarLauncher
{
    public partial class MoveAppDialog : Window
    {
        public GroupConfig? SelectedGroup { get; private set; }

        public MoveAppDialog(List<GroupConfig> groups)
        {
            InitializeComponent();
            GroupListBox.ItemsSource = groups;
            GroupListBox.DisplayMemberPath = "Name";
        }

        private void Move_Click(object sender, RoutedEventArgs e)
        {
            if (GroupListBox.SelectedItem is GroupConfig selected)
            {
                SelectedGroup = selected;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show(
                    "移動先のグループを選択してください。",
                    "未選択",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}