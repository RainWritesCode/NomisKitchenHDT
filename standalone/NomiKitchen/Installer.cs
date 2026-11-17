using System.Diagnostics;
using System.Reflection;

namespace NomiKitchen;

static class Installer
{
    public static string PluginsDir(string hs) => Path.Combine(hs, "BepInEx", "plugins");

    public static bool IsInstalled(string hs, PluginInfo p) => File.Exists(Path.Combine(PluginsDir(hs), p.Dll));

    public static bool HearthstoneRunning() => Process.GetProcessesByName("Hearthstone").Length > 0;

    public static void Install(string hs, PluginInfo p)
    {
        var dir = PluginsDir(hs);
        if (!Directory.Exists(dir))
            throw new InvalidOperationException($"BepInEx plugins folder not found: {dir}. Install BepInEx 5 into the Hearthstone folder first.");

        using var src = Assembly.GetExecutingAssembly().GetManifestResourceStream(p.Dll)
            ?? throw new InvalidOperationException($"Embedded mod missing: {p.Dll}");
        using var dst = File.Create(Path.Combine(dir, p.Dll));
        src.CopyTo(dst);
        ClearTypeCache(hs);
    }

    public static void Uninstall(string hs, PluginInfo p)
    {
        var file = Path.Combine(PluginsDir(hs), p.Dll);
        if (File.Exists(file)) File.Delete(file);
        ClearTypeCache(hs);
    }

    // BepInEx caches plugin types; a stale cache keeps a removed or updated mod loading.
    static void ClearTypeCache(string hs)
    {
        var cache = Path.Combine(hs, "BepInEx", "cache", "chainloader_typeloader.dat");
        if (File.Exists(cache)) File.Delete(cache);
    }
}
