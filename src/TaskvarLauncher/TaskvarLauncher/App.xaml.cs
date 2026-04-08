using System.Windows;
using Application = System.Windows.Application;

namespace TaskbarLauncher
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string[] args = e.Args;

            if (args.Length >= 2 && args[0] == "--group")
            {
                string groupId = args[1];
                var popup = new PopupWindow(groupId);
                popup.Show();
            }
            else
            {
                var main = new MainWindow();
                main.Show();
            }
        }
    }
}