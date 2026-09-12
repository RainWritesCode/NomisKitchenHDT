using System;
using System.IO;
using Newtonsoft.Json;

namespace NomisKitchenHDT
{
    public class PluginConfig
    {
        public bool DisableAbbreviation { get; set; } = false;
        public bool ShowApmOverlay { get; set; } = true;
        public bool ShowGamePeak { get; set; } = true;

        public double OverlayX { get; set; } = 40;
        public double OverlayY { get; set; } = 40;

        public string HearthstoneDir { get; set; } = "";

        public string FontFamily { get; set; } = "Segoe UI";
        public double LabelFontSize { get; set; } = 11;
        public double ValueFontSize { get; set; } = 22;
        public string BackgroundColor { get; set; } = "#EE1A1A1A";
        public string BorderColor { get; set; } = "#33FFFFFF";
        public string LabelColor { get; set; } = "#B8B8B8";
        public string ValueColor { get; set; } = "#F5A623";
        public int DecimalPlaces { get; set; } = 1;
        public double OverlayScale { get; set; } = 1.0;
        public bool LockOverlay { get; set; } = false;
        public bool AutoCheckUpdates { get; set; } = true;

        static string ConfigPath =>
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
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented));
            }
            catch { }
        }
    }
}
