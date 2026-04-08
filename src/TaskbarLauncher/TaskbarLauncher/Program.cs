using System;

namespace TaskbarLauncher
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // 引数チェック（WPFの重いライブラリを読み込む前に行う！）
            bool isGroupLaunch = args.Length >= 2 && args[0] == "--group";

            if (isGroupLaunch)
            {
                string groupId = args[1];

                // メインアプリへの通知を試みる
                if (NamedPipeClient.SendGroupIdToRunningInstance(groupId))
                {
                    // 通知に成功したら、WPFUIを一切起動せずに超高速でこのプロセスを終了する
                    return;
                }
            }

            // グループ起動ではない、またはメインアプリが起動していなかった場合は
            // 通常通りWPFアプリケーションを立ち上げる
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}