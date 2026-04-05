using System.Collections.Generic;

namespace TaskbarLauncher.Models
{
    public class GroupConfig
    {
        public string Name { get; set; } = "";
        public List<AppConfig> Apps { get; set; } = new List<AppConfig>();
    }
}