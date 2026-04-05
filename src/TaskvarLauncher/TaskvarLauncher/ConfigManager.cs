using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TaskbarLauncher.Models;

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
    }
}