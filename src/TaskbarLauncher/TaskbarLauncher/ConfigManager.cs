using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.Json;
using System.Windows;
using TaskbarLauncher.Models;
using MessageBox = System.Windows.MessageBox;

namespace TaskbarLauncher
{
    public class ConfigManager
    {
        private static readonly string ConfigPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StackBar", "groups.json");

        private static readonly string IconCacheDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StackBar", "icons");

        public List<GroupConfig> LoadGroups()
        {
            if (!File.Exists(ConfigPath))
                return new List<GroupConfig>();

            string json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<List<GroupConfig>>(json) ?? new List<GroupConfig>();
        }

        public void SaveGroups(List<GroupConfig> groups)
        {
            string dir = Path.GetDirectoryName(ConfigPath)!;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string json = JsonSerializer.Serialize(groups, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
        }

        //アプリのアイコン画像を取得する
        private Bitmap? GetAppIcon(string exePath)
        {
            try
            {
                var icon = System.Drawing.Icon.ExtractAssociatedIcon(exePath);
                return icon?.ToBitmap();
            }
            catch
            {
                return null;
            }
        }

        //グループ内のアプリアイコンを合成して .ico ファイルを生成する
        public string? CreateGroupIcon(GroupConfig group)
        {
            if (!Directory.Exists(IconCacheDir))
                Directory.CreateDirectory(IconCacheDir);
            //古いアイコンを削除
            string oldIcoPath = Path.Combine(IconCacheDir, $"{group.Id}.ico");
            if (File.Exists(oldIcoPath))
                File.Delete(oldIcoPath);

            //アプリのパスリストを取得（最大4個）
            var apps = group.Apps;
            var bitmaps = new List<Bitmap>();

            foreach (var app in apps)
            {
                if (bitmaps.Count >= 4) break;
                var bmp = GetAppIcon(app.Path);
                if (bmp != null)
                    bitmaps.Add(bmp);
            }

            if (bitmaps.Count == 0)
                return null;

            //256×256の合成画像を作る
            int size = 256;
            var canvas = new Bitmap(size, size);
            using var g = Graphics.FromImage(canvas);
            g.Clear(Color.Transparent);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            //アプリ数に応じてレイアウトを変える
            int count = bitmaps.Count;
            if (count == 1)
            {
                //1個：中央に大きく
                g.DrawImage(bitmaps[0], 16, 16, 224, 224);
            }
            else if (count == 2)
            {
                //2個：左右に並べる
                g.DrawImage(bitmaps[0], 4, 52, 120, 120);
                g.DrawImage(bitmaps[1], 132, 52, 120, 120);
            }
            else
            {
                //3〜4個：2×2グリッド
                int cell = 120;
                int padding = 4;
                int[] xs = { padding, size / 2 + padding / 2 };
                int[] ys = { padding, size / 2 + padding / 2 };

                for (int i = 0; i < Math.Min(count, 4); i++)
                {
                    int col = i % 2;
                    int row = i / 2;
                    g.DrawImage(bitmaps[i], xs[col], ys[row], cell, cell);
                }
            }

            //.ico ファイルとして保存
            string icoPath = Path.Combine(IconCacheDir, $"{group.Id}.ico");
            SaveAsIco(canvas, icoPath);

            //使い終わったBitmapを解放
            foreach (var bmp in bitmaps)
                bmp.Dispose();
            canvas.Dispose();

            return icoPath;
        }

        //Bitmap を .ico ファイルとして保存する
        private void SaveAsIco(Bitmap bitmap, string path)
        {
            //256×256 に縮小
            using var resized = new Bitmap(bitmap, new System.Drawing.Size(256, 256));
            using var ms = new MemoryStream();
            resized.Save(ms, ImageFormat.Png);
            byte[] pngData = ms.ToArray();

            using var fs = new FileStream(path, FileMode.Create);
            using var writer = new BinaryWriter(fs);

            //ICO ヘッダー
            writer.Write((short)0);
            writer.Write((short)1);
            writer.Write((short)1);

            //画像ディレクトリエントリ
            writer.Write((byte)0); 
            writer.Write((byte)0); 
            writer.Write((byte)0); 
            writer.Write((byte)0); 
            writer.Write((short)1);
            writer.Write((short)32); 
            writer.Write(pngData.Length);
            writer.Write(22);

            // PNG データ本体
            writer.Write(pngData);
        }

        public void CreateShortcut(GroupConfig group)
        {
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location
                .Replace(".dll", ".exe");
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string shortcutPath = Path.Combine(desktopPath, $"{group.Name}.lnk");

            // グループアイコンを生成
            string? icoPath = CreateGroupIcon(group);

            // アイコン指定の部分（生成できた場合のみ追加）
            string iconLine = icoPath != null
                ? $"$shortcut.IconLocation = '{icoPath},0'; "
                : "";

            string script = $@"$shell = New-Object -ComObject WScript.Shell; $shortcut = $shell.CreateShortcut('{shortcutPath}'); $shortcut.TargetPath = '{exePath}'; $shortcut.Arguments = '--group {group.Id}'; $shortcut.Description = 'StackBar - {group.Name}'; {iconLine}$shortcut.Save()";

            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -Command \"{script}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true
                }
            };

            process.Start();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            // Windowsのアイコンキャッシュをリフレッシュ
            if (process.ExitCode == 0)
            {
                System.Threading.Thread.Sleep(500);
                SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
            }

            if (process.ExitCode != 0)
            {
                MessageBox.Show(
                    $"ショートカット作成に失敗しました。\n{error}",
                    "エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    [System.Runtime.InteropServices.DllImport("shell32.dll")]
        private static extern void SHChangeNotify(int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);
    }
    }