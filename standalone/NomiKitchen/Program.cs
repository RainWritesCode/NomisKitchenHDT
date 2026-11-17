namespace NomiKitchen;

static class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        string? hs = null;
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == "--hs") hs = args[i + 1];

        // Headless check used by testing: exercises install, uninstall and config edits.
        if (args.Length >= 2 && args[0] == "--selftest") return SelfTest(args[1]);

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm(hs));
        return 0;
    }

    static int SelfTest(string hs)
    {
        int fails = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine($"{(ok ? "PASS" : "FAIL")} {what}");
            if (!ok) fails++;
        }

        foreach (var p in Plugins.All)
        {
            Installer.Install(hs, p);
            Check(Installer.IsInstalled(hs, p), $"{p.Id}: installed");
            Installer.Install(hs, p);
            Check(Installer.IsInstalled(hs, p), $"{p.Id}: reinstall is idempotent");
            Installer.Uninstall(hs, p);
            Check(!Installer.IsInstalled(hs, p), $"{p.Id}: uninstalled");
        }

        var dance = Plugins.All[0];
        var cfg = Cfg.PathFor(hs, dance.Cfg!);
        Cfg.Set(cfg, "Fix", "Enabled", "false");
        Check(Cfg.Get(cfg, "Fix", "Enabled") == "false", "cfg: Enabled set to false");
        Check(Cfg.Get(cfg, "Fix", "LogFixes") == "true", "cfg: LogFixes untouched");
        Check(Cfg.Get(cfg, "General", "Name") is not null, "cfg: other sections kept");
        Cfg.Set(cfg, "Fix", "Enabled", "true");
        Check(Cfg.Get(cfg, "Fix", "Enabled") == "true", "cfg: Enabled back to true");

        try { Cfg.Set(cfg, "Fix", "NoSuchKey", "x"); Check(false, "cfg: missing key throws"); }
        catch (InvalidOperationException) { Check(true, "cfg: missing key throws"); }

        Console.WriteLine(fails == 0 ? "ALL PASS" : $"{fails} FAILED");
        return fails == 0 ? 0 : 1;
    }
}
