using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TaskbarLauncher.Models;
using System.Windows;

namespace TaskbarLauncher
{
    public class ConfigManager
    {
        private static readonly string ConfigPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StackBar", "groups.json");

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

        public void CreateShortcut(GroupConfig group)
        {
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location
                .Replace(".dll", ".exe");
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string shortcutPath = System.IO.Path.Combine(desktopPath, $"{group.Name}.lnk");

            string script = $@"$shell = New-Object -ComObject WScript.Shell; $shortcut = $shell.CreateShortcut('{shortcutPath}'); $shortcut.TargetPath = '{exePath}'; $shortcut.Arguments = '--group {group.Id}'; $shortcut.Description = 'StackBar - {group.Name}'; $shortcut.Save()";

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

            if (process.ExitCode != 0)
            {
                MessageBox.Show(
                    $"ショートカット作成に失敗しました。\n{error}",
                    "エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}