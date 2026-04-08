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

            // ========================
            // 【重要】ここから追加
            // ========================

            // コマンドライン引数を取得（WPF の e.Args ではなく System 関数を使用）
            string[] args = Environment.GetCommandLineArgs().Skip(1).ToArray(); // 最初は exe パス なのでスキップ

            // 引数を確認（デバッグ用）
            System.Diagnostics.Debug.WriteLine($"[App.OnStartup] 引数の数: {args.Length}");
            for (int i = 0; i < args.Length; i++)
            {
                System.Diagnostics.Debug.WriteLine($"[App.OnStartup] Args[{i}]: {args[i]}");
            }

            // ShutdownMode を設定（タスクトレイのみで常駐）
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // グループを指定して起動した場合
            bool isGroupLaunch = args.Length >= 2 && args[0] == "--group";
            string? groupId = null;

            if (isGroupLaunch)
            {
                groupId = args[1];
                System.Diagnostics.Debug.WriteLine($"[App.OnStartup] グループ指定で起動: {groupId}");

                // 常駐中のメインアプリに通知を試みる
                if (NamedPipeClient.SendGroupIdToRunningInstance(groupId))
                {
                    // 通知成功 → この exe は終了
                    System.Diagnostics.Debug.WriteLine("[App.OnStartup] メインアプリへの通知成功。この exe は終了します");
                    Shutdown(0);
                    return;
                }

                // 通知失敗 → 初回起動（メインアプリはまだ起動していない）
                System.Diagnostics.Debug.WriteLine("[App.OnStartup] メインアプリが起動していません。初回起動として処理します");
            }

            // ========================
            // タスクトレイアイコンの初期化
            // ========================
            var notifyIcon = new System.Windows.Forms.NotifyIcon();

            // アイコンファイルを試す、なければデフォルトアイコンを使用
            try
            {
                if (System.IO.File.Exists("taskbar_icon.ico"))
                {
                    notifyIcon.Icon = new System.Drawing.Icon("taskbar_icon.ico");
                }
                else
                {
                    // デフォルトアイコン（システムのアプリケーションアイコン）を使用
                    notifyIcon.Icon = System.Drawing.SystemIcons.Application;
                }
            }
            catch
            {
                // アイコン読み込み失敗時はシステムアイコンを使用
                notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            }

            notifyIcon.Visible = true;
            notifyIcon.Text = "StackBar";

            // 右クリックメニュー
            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            contextMenu.Items.Add("設定を開く", null, (s, ea) =>
            {
                MainWindow?.Activate();
            });
            contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            contextMenu.Items.Add("終了", null, (s, ea) =>
            {
                Shutdown();
            });
            notifyIcon.ContextMenuStrip = contextMenu;

            // ダブルクリック
            notifyIcon.DoubleClick += (s, ea) =>
            {
                MainWindow?.Activate();
            };

            // アプリ終了時にアイコンを削除
            Exit += (s, ea) =>
            {
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
                NamedPipeServer.StopListening();
            };

            // ========================
            // パイプサーバーの起動
            // ========================
            NamedPipeServer.StartListening();
            System.Diagnostics.Debug.WriteLine("[App.OnStartup] パイプサーバーを起動しました");

            // ========================
            // ウィンドウの表示
            // ========================
            if (isGroupLaunch && groupId != null)
            {
                // グループ指定で起動 → ポップアップウィンドウを表示
                var popup = new PopupWindow(groupId);
                popup.Show();
                System.Diagnostics.Debug.WriteLine($"[App.OnStartup] ポップアップウィンドウを表示: {groupId}");
            }
            else
            {
                // 通常起動 → 設定ウィンドウを表示
                if (MainWindow == null)
                {
                    MainWindow = new MainWindow();
                }
                MainWindow.Show();
            }
        }

        // ... 既存のコード ...
    }
}