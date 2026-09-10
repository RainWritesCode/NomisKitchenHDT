using System;
using System.IO;
using System.Reflection;
using NomisKitchenHDT.Utils;

namespace NomisKitchenHDT.Services
{
    public class AbbreviationDisabler : IDisposable
    {
        private const string EmbeddedResourceName =
            "NomisKitchenHDT.Resources.com.community.hs.NomiHatesAbbreviation.dll";
        private const string DeployedFileName =
            "com.community.hs.NomiHatesAbbreviation.dll";

        private readonly PluginConfig _config;

        public AbbreviationDisabler(PluginConfig config)
        {
            _config = config;
        }

        public string ResolvePluginsFolder()
        {
            if (!string.IsNullOrEmpty(_config.HearthstoneDir))
            {
                var p = Path.Combine(_config.HearthstoneDir, "BepInEx", "plugins");
                if (Directory.Exists(p)) return p;
            }
            foreach (var candidate in new[]
            {
                @"C:\Program Files (x86)\Hearthstone",
                @"C:\Program Files\Hearthstone",
            })
            {
                var p = Path.Combine(candidate, "BepInEx", "plugins");
                if (Directory.Exists(p)) return p;
            }
            return null;
        }

        public void SyncWithSetting()
        {
            var pluginsFolder = ResolvePluginsFolder();
            if (pluginsFolder == null) { Log.Warn("Abbreviation-disabler: no Hearthstone BepInEx\\plugins folder found (config HearthstoneDir='" + _config.HearthstoneDir + "')."); return; }

            var target = Path.Combine(pluginsFolder, DeployedFileName);
            try
            {
                if (_config.DisableAbbreviation) ExtractIfMissing(target);
                else DeleteIfPresent(target);
            }
            catch (Exception ex) { Log.Error("Abbreviation-disabler sync failed", ex); }
        }

        private void ExtractIfMissing(string target)
        {
            if (File.Exists(target)) { Log.Info("Abbreviation-disabler dll already present: " + target); return; }
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(EmbeddedResourceName);
            if (stream == null) return;
            using var file = File.Create(target);
            stream.CopyTo(file);
            Log.Info("Abbreviation-disabler dll extracted to: " + target);

            var cache = Path.Combine(Path.GetDirectoryName(target) ?? "", "..", "cache", "chainloader_typeloader.dat");
            var full = Path.GetFullPath(cache);
            if (File.Exists(full)) File.Delete(full);
        }

        private void DeleteIfPresent(string target)
        {
            if (File.Exists(target)) { File.Delete(target); Log.Info("Abbreviation-disabler dll removed: " + target); }
        }

        public void Dispose() { }
    }
}
