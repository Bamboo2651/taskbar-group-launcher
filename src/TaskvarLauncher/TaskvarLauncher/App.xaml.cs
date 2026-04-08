using System;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace TaskbarLauncher
{
    public partial class App : Application
    {
        private NotifyIcon? _notifyIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // タスクトレイアイコンを設定
            _notifyIcon = new NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Application,
                Visible = true,
                Text = "StackBar"
            };

            // 右クリックメニュー
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("設定を開く", null, (s, ev) =>
            {
                var main = new MainWindow();
                main.Show();
                main.Activate();
            });
            contextMenu.Items.Add("-"); // 区切り線
            contextMenu.Items.Add("終了", null, (s, ev) =>
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                Shutdown();
            });

            _notifyIcon.ContextMenuStrip = contextMenu;

            // ダブルクリックで設定を開く
            _notifyIcon.DoubleClick += (s, ev) =>
            {
                var main = new MainWindow();
                main.Show();
                main.Activate();
            };

            string[] args = e.Args;

            if (args.Length >= 2 && args[0] == "--group")
            {
                string groupId = args[1];
                var popup = new PopupWindow(groupId);
                popup.Show();
            }
            else
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown;
                var main = new MainWindow();
                main.Show();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _notifyIcon?.Dispose();
            base.OnExit(e);
        }
    }
}