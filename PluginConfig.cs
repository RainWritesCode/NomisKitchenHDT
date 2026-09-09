using System;
using System.IO;
using Newtonsoft.Json;

namespace NomisKitchenHDT
{
    public class PluginConfig
    {
        public bool DisableAbbreviation { get; set; } = false;
        public bool ShowApmOverlay { get; set; } = true;

        public double OverlayX { get; set; } = 40;
        public double OverlayY { get; set; } = 40;

        public string HearthstoneDir { get; set; } = "";

        private static string ConfigPath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "HearthstoneDeckTracker",
                "NomisKitchenHDT.config.json");

        public static PluginConfig Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    return JsonConvert.DeserializeObject<PluginConfig>(json) ?? new PluginConfig();
                }
            }
            catch { }
            return new PluginConfig();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
            catch { }
        }
    }
}
