using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using TaskbarLauncher.Models;

namespace TaskbarLauncher
{
    public class NamedPipeServer
    {
        private const string PipeName = "StackBar_MutualExclusion";
        private static bool _isListening = false;
        private static List<GroupConfig> _cachedGroups = new List<GroupConfig>();

        public static void SetCachedGroups(List<GroupConfig> groups)
        {
            _cachedGroups = groups;
        }

        public static void StartListening()
        {
            if (_isListening) return;
            _isListening = true;
            Task.Run(() => WaitForNextConnection());
            System.Diagnostics.Debug.WriteLine("[NamedPipeServer] パイプサーバー起動");
        }

        private static void WaitForNextConnection()
        {
            if (!_isListening) return;

            try
            {
                // maxNumberOfServerInstances を増やして同時接続に備える
                var server = new NamedPipeServerStream(
                    PipeName, PipeDirection.In,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                server.BeginWaitForConnection(ar =>
                {
                    try
                    {
                        server.EndWaitForConnection(ar);

                        // 接続が来たら即座に次の待ち受けを開始
                        Task.Run(() => WaitForNextConnection());

                        using (server)
                        using (var reader = new StreamReader(server))
                        {
                            string groupId = reader.ReadLine();
                            if (!string.IsNullOrEmpty(groupId))
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    ShowPopup(groupId);
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[NamedPipeServer] 接続処理エラー: {ex.Message}");
                        Task.Run(() => WaitForNextConnection());
                    }
                }, null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NamedPipeServer] 待ち受けエラー: {ex.Message}");
                Thread.Sleep(100);
                Task.Run(() => WaitForNextConnection());
            }
        }

        private static PopupWindow? _currentPopup = null;

        private static void ShowPopup(string groupId)
        {
            // 既存のポップアップを先に閉じる
            if (_currentPopup != null)
            {
                try { _currentPopup.Close(); } catch { }
                _currentPopup = null;
            }

            var popup = new PopupWindow(groupId, _cachedGroups);
            _currentPopup = popup;

            popup.Closed += (s, e) =>
            {
                if (_currentPopup == popup)
                    _currentPopup = null;
            };

            popup.Show();
            System.Diagnostics.Debug.WriteLine($"[NamedPipeServer] ポップアップ表示: {groupId}");
        }

        public static void StopListening()
        {
            _isListening = false;
            System.Diagnostics.Debug.WriteLine("[NamedPipeServer] パイプサーバー停止");
        }
    }
}