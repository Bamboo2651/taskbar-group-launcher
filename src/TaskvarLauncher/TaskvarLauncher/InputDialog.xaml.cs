using System.Printing;
using System.Windows;

namespace TaskbarLauncher
{
    public partial class InputDialog : Window
    {
        public string Answer { get; private set; } = "";

        public InputDialog(string title)
        {
            InitializeComponent();
            TitleText.Text = title;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputBox.Text))
            {
                InputBox.BorderBrush = System.Windows.Media.Brushes.Red;
                return;
            }
            Answer = InputBox.Text.Trim();
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}