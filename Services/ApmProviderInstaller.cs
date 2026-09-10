using System;
using System.IO;
using System.Reflection;
using NomisKitchenHDT.Utils;

namespace NomisKitchenHDT.Services
{
    public class ApmProviderInstaller
    {
        const string EmbeddedResourceName =
            "NomisKitchenHDT.Resources.com.community.hs.NomisKitchenApm.dll";
        const string DeployedFileName =
            "com.community.hs.NomisKitchenApm.dll";

        readonly PluginConfig _config;

        public ApmProviderInstaller(PluginConfig config)
        {
            _config = config;
        }

        public void EnsureInstalled()
        {
            var pluginsFolder = ResolvePluginsFolder();
            if (pluginsFolder == null) { Log.Warn("APM provider install: no Hearthstone BepInEx\\plugins folder found (config HearthstoneDir='" + _config.HearthstoneDir + "'). BepInEx is not installed, or the Hearthstone folder is wrong."); return; }
            var target = Path.Combine(pluginsFolder, DeployedFileName);
            if (File.Exists(target)) { Log.Info("APM provider dll already present: " + target); return; }

            try
            {
                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(EmbeddedResourceName);
                if (stream == null) { Log.Error("APM provider: embedded resource missing: " + EmbeddedResourceName); return; }
                using var file = File.Create(target);
                stream.CopyTo(file);
                Log.Info("APM provider dll extracted to: " + target);

                var cache = Path.Combine(Path.GetDirectoryName(target) ?? "", "..", "cache", "chainloader_typeloader.dat");
                var full = Path.GetFullPath(cache);
                if (File.Exists(full)) File.Delete(full);
            }
            catch (Exception ex) { Log.Error("APM provider install failed", ex); }
        }

        string ResolvePluginsFolder()
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
    }
}
