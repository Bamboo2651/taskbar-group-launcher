using System;
using System.Collections.Concurrent;
using System.IO.Pipes;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TaskbarLauncher
{
    /// <summary>
    /// 常駐中のメインアプリがグループ開く命令を受け取るためのクラス
    /// </summary>
    public class NamedPipeServer
    {
        private const string PipeName = "StackBar_MutualExclusion";
        private static ConcurrentQueue<string> _groupIdQueue = new ConcurrentQueue<string>();
        private static DispatcherTimer _queueCheckTimer;
        private static bool _isListening = false;

        /// <summary>
        /// パイプサーバーのリッスンを開始（メインアプリの OnStartup で呼び出す）
        /// </summary>
        public static void StartListening()
        {
            if (_isListening)
                return;

            _isListening = true;

            // バックグラウンドでパイプ受信を開始
            Task.Run(() => ListenForConnections());

            // DispatcherTimer でキューをチェック（100ms ごと）
            _queueCheckTimer = new DispatcherTimer();
            _queueCheckTimer.Interval = TimeSpan.FromMilliseconds(100);
            _queueCheckTimer.Tick += (s, e) => ProcessQueue();
            _queueCheckTimer.Start();

            System.Diagnostics.Debug.WriteLine("[NamedPipeServer] パイプサーバーのリッスンを開始しました");
        }

        /// <summary>
        /// パイプ受信ループ（別スレッドで実行）
        /// </summary>
        private static void ListenForConnections()
        {
            while (_isListening)
            {
                try
                {
                    // パイプを作成
                    using (var server = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte))
                    {
                        // クライアント接続を待つ
                        server.WaitForConnection();

                        // グループID を読み込む
                        using (var reader = new StreamReader(server))
                        {
                            string groupId = reader.ReadLine();
                            if (!string.IsNullOrEmpty(groupId))
                            {
                                // キューに追加
                                _groupIdQueue.Enqueue(groupId);
                                System.Diagnostics.Debug.WriteLine($"[NamedPipeServer] グループ {groupId} を受け取りました");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[NamedPipeServer] エラー: {ex.Message}");
                    // パイプエラーが発生しても、リッスンを続ける
                    Thread.Sleep(100);
                }
            }
        }

        /// <summary>
        /// キューをチェックして PopupWindow を表示（メインスレッドで実行）
        /// </summary>
        private static void ProcessQueue()
        {
            while (_groupIdQueue.TryDequeue(out string groupId))
            {
                try
                {
                    // PopupWindow を表示
                    var popup = new PopupWindow(groupId);
                    popup.Show();
                    System.Diagnostics.Debug.WriteLine($"[NamedPipeServer] ポップアップを表示しました: {groupId}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[NamedPipeServer] ポップアップ表示エラー: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// パイプサーバーのリッスンを停止（アプリ終了時に呼び出す）
        /// </summary>
        public static void StopListening()
        {
            _isListening = false;

            // タイマーを停止
            if (_queueCheckTimer != null)
            {
                _queueCheckTimer.Stop();
                _queueCheckTimer = null;
            }

            // キューをクリア
            while (_groupIdQueue.TryDequeue(out _)) { }

            System.Diagnostics.Debug.WriteLine("[NamedPipeServer] パイプサーバーのリッスンを停止しました");
        }
    }
}