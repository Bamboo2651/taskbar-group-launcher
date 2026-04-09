using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using SystemApplication = System.Windows.Application;

namespace TaskbarLauncher
{
    public partial class App : SystemApplication
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string[] args = Environment.GetCommandLineArgs().Skip(1).ToArray();

            System.Diagnostics.Debug.WriteLine($"[App.OnStartup] 引数の数: {args.Length}");
            for (int i = 0; i < args.Length; i++)
            {
                System.Diagnostics.Debug.WriteLine($"[App.OnStartup] Args[{i}]: {args[i]}");
            }

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            bool isGroupLaunch = args.Length >= 2 && args[0] == "--group";
            string? groupId = null;

            //if (isGroupLaunch)
            //{
            //    groupId = args[1];
            //    System.Diagnostics.Debug.WriteLine($"[App.OnStartup] グループ指定で起動: {groupId}");

            //    if (NamedPipeClient.SendGroupIdToRunningInstance(groupId))
            //    {
            //        System.Diagnostics.Debug.WriteLine("[App.OnStartup] メインアプリへの通知成功。この exe は終了します");
            //        Shutdown(0);
            //        return;
            //    }

            //    System.Diagnostics.Debug.WriteLine("[App.OnStartup] メインアプリが起動していません。初回起動として処理します");
            //}

            //起動時に1回だけ設定を読み込んでキャッシュする
            var configManager = new ConfigManager();
            var groups = configManager.LoadGroups();
            NamedPipeServer.SetCachedGroups(groups);

            //タスクトレイアイコンの初期化
            var notifyIcon = new System.Windows.Forms.NotifyIcon();
            try
            {
                notifyIcon.Icon = System.IO.File.Exists("taskbar_icon.ico")
                    ? new System.Drawing.Icon("taskbar_icon.ico")
                    : System.Drawing.SystemIcons.Application;
            }
            catch
            {
                notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            }

            notifyIcon.Visible = true;
            notifyIcon.Text = "StackBar";

            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            contextMenu.Items.Add("設定を開く", null, (s, ea) => MainWindow?.Activate());
            contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            contextMenu.Items.Add("終了", null, (s, ea) => Shutdown());
            notifyIcon.ContextMenuStrip = contextMenu;

            notifyIcon.DoubleClick += (s, ea) => MainWindow?.Activate();

            Exit += (s, ea) =>
            {
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
                NamedPipeServer.StopListening();
            };

            NamedPipeServer.StartListening();
            System.Diagnostics.Debug.WriteLine("[App.OnStartup] パイプサーバーを起動しました");

            if (isGroupLaunch && groupId != null)
            {
                var popup = new PopupWindow(groupId, groups);
                popup.Show();
                popup.Topmost = true;
                popup.Activate();
                popup.Focus();
                System.Diagnostics.Debug.WriteLine($"[App.OnStartup] ポップアップウィンドウを表示: {groupId}");
            }
            else
            {
                if (MainWindow == null)
                {
                    MainWindow = new MainWindow();
                }
                MainWindow.Show();
            }
        }
    }
}