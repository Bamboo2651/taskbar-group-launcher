using System.Windows;

namespace TaskbarLauncher
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 引数を確認する
            string[] args = e.Args;

            if (args.Length >= 2 && args[0] == "--group")
            {
                // グループIDが渡された場合はポップアップを表示
                string groupId = args[1];
                var popup = new PopupWindow(groupId);
                popup.Show();
            }
            else
            {
                // 引数がない場合は設定アプリを表示
                var main = new MainWindow();
                main.Show();
            }
        }
    }
}