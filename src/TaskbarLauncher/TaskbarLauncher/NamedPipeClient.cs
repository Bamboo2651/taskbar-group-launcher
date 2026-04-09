using System;
using System.IO.Pipes;
using System.IO;
using System.Threading.Tasks;

namespace TaskbarLauncher
{
    /// <summary>
    /// 新しく起動した exe がメインアプリ（常駐 exe）に通知を送るためのクラス
    /// </summary>
    public class NamedPipeClient
    {
        private const string PipeName = "StackBar_MutualExclusion";
        private const int TimeoutMs = 1000; // 1秒でタイムアウト

        /// <summary>
        /// 常駐中のメインアプリにグループID を送信
        /// </summary>
        /// <param name="groupId">開きたいグループのID</param>
        /// <returns>true: 通知成功（メインアプリが応答した）, false: 通知失敗（メインアプリが起動していない）</returns>
        public static bool SendGroupIdToRunningInstance(string groupId)
        {
            try
            {
                // パイプに接続を試みる
                using (var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.None))
                {
                    // 1秒以内に接続できるか試す
                    var connectTask = client.ConnectAsync(TimeoutMs);

                    // タスクの完了を待つ（タイムアウト付き）
                    if (!connectTask.Wait(TimeoutMs))
                    {
                        // 1秒以内に接続できなかった → メインアプリは起動していない
                        System.Diagnostics.Debug.WriteLine("[NamedPipeClient] 接続タイムアウト（メインアプリが起動していない）");
                        return false;
                    }

                    // 接続成功 → グループID を送信
                    using (var writer = new StreamWriter(client))
                    {
                        writer.WriteLine(groupId);
                        writer.Flush();
                    }

                    System.Diagnostics.Debug.WriteLine($"[NamedPipeClient] グループID {groupId} を送信しました");
                    return true; // 通知成功
                }
            }
            catch (TimeoutException)
            {
                // タイムアウト → メインアプリは起動していない
                System.Diagnostics.Debug.WriteLine("[NamedPipeClient] 接続タイムアウト例外");
                return false;
            }
            catch (Exception ex)
            {
                // 接続失敗（例外発生） → メインアプリは起動していない
                System.Diagnostics.Debug.WriteLine($"[NamedPipeClient] 通知失敗: {ex.Message}");
                return false;
            }
        }
        public static bool SendMessageToRunningInstance(string message)
        {
            try
            {
                using (var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.None))
                {
                    if (!client.ConnectAsync(TimeoutMs).Wait(TimeoutMs)) return false;
                    using (var writer = new StreamWriter(client))
                    {
                        writer.WriteLine(message);
                        writer.Flush();
                    }
                    return true;
                }
            }
            catch { return false; }
        }  
    }
}