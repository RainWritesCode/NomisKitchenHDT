using System;
using System.IO;
using System.Reflection;

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
                @"I:\Hearthstone",
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
            if (pluginsFolder == null) return;

            var target = Path.Combine(pluginsFolder, DeployedFileName);
            try
            {
                if (_config.DisableAbbreviation) ExtractIfMissing(target);
                else DeleteIfPresent(target);
            }
            catch { }
        }

        private void ExtractIfMissing(string target)
        {
            if (File.Exists(target)) return;
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(EmbeddedResourceName);
            if (stream == null) return;
            using var file = File.Create(target);
            stream.CopyTo(file);

            var cache = Path.Combine(Path.GetDirectoryName(target) ?? "", "..", "cache", "chainloader_typeloader.dat");
            var full = Path.GetFullPath(cache);
            if (File.Exists(full)) File.Delete(full);
        }

        private void DeleteIfPresent(string target)
        {
            if (File.Exists(target)) File.Delete(target);
        }

        public void Dispose() { }
    }
}
