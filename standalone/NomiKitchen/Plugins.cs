namespace NomiKitchen;

// One entry per BepInEx mod. To add a mod: embed its DLL in the csproj and add a record here.
record CfgOption(string Section, string Key, string Label);

record PluginInfo(string Id, string Title, string Blurb, string Dll, string? Cfg, CfgOption[] Options);

static class Plugins
{
    public static readonly PluginInfo[] All =
    {
        new("dance", "Minion dance fix",
            "Stops minions from swapping positions and dancing out of place during Battlegrounds.",
            "com.community.hs.NomiCantDance.dll", "com.community.hs.NomiCantDance.cfg",
            new[]
            {
                new CfgOption("Fix", "Enabled", "Enable the board fix"),
                new CfgOption("Fix", "LogFixes", "Log each corrected slot to LogOutput.log"),
            }),
        new("abbrev", "Disable abbreviation",
            "Shows full attack and health numbers (no 1.2k or 5M). Installing turns it on; uninstalling turns it off.",
            "com.community.hs.NomiHatesAbbreviation.dll", null, Array.Empty<CfgOption>()),
        new("apm", "APM provider",
            "Publishes your live APM to shared memory for overlays. This alone shows nothing on screen.",
            "com.community.hs.NomisKitchenApm.dll", "com.community.hs.NomisKitchenApm.cfg",
            new[]
            {
                new CfgOption("Debug", "VerifySampler", "Slow debug check: compare APM counts with a full scan"),
            }),
    };
}
