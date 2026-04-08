using System;
using System.Collections.Generic;

namespace TaskbarLauncher.Models
{
    public class GroupConfig
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public List<AppConfig> Apps { get; set; } = new List<AppConfig>();
    }
}