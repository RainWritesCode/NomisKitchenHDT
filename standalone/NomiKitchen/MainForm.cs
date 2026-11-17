namespace NomiKitchen;

sealed class MainForm : Form
{
    readonly TextBox _hsDir = new() { Width = 380 };
    readonly Label _status = new() { AutoSize = true, Location = new Point(10, 10) };
    readonly Label _running = new() { AutoSize = true, Location = new Point(10, 40) };
    readonly List<Action> _refreshers = new();

    string HsDir => _hsDir.Text.Trim();

    public MainForm(string? hsDir)
    {
        Text = "Nomi's Kitchen - Standalone";
        ClientSize = new Size(560, 390);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9f);

        _hsDir.Text = hsDir ?? @"C:\Program Files (x86)\Hearthstone";
        _hsDir.TextChanged += (_, _) => RefreshAll();

        var browse = new Button { Text = "Browse...", AutoSize = true };
        browse.Click += (_, _) =>
        {
            using var dlg = new FolderBrowserDialog { SelectedPath = HsDir };
            if (dlg.ShowDialog(this) == DialogResult.OK) _hsDir.Text = dlg.SelectedPath;
        };

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8, 8, 0, 0) };
        top.Controls.Add(new Label { Text = "Hearthstone folder:", AutoSize = true, Padding = new Padding(0, 4, 0, 0) });
        top.Controls.Add(_hsDir);
        top.Controls.Add(browse);

        var tabs = new TabControl { Dock = DockStyle.Fill };
        foreach (var p in Plugins.All) tabs.TabPages.Add(BuildTab(p));

        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 64 };
        bottom.Controls.Add(_status);
        bottom.Controls.Add(_running);

        // Fill first, then edge-docked panels, so the tabs take the remaining space.
        Controls.Add(tabs);
        Controls.Add(bottom);
        Controls.Add(top);

        RefreshAll();
    }

    TabPage BuildTab(PluginInfo p)
    {
        var page = new TabPage(p.Title) { Padding = new Padding(12) };
        var state = new Label { Location = new Point(12, 12), AutoSize = true };
        var install = new Button { Text = "Install", Location = new Point(12, 40), Size = new Size(110, 30) };
        var uninstall = new Button { Text = "Uninstall", Location = new Point(130, 40), Size = new Size(110, 30) };
        var blurb = new Label { Text = p.Blurb, Location = new Point(12, 80), Size = new Size(500, 36) };
        page.Controls.AddRange(new Control[] { state, install, uninstall, blurb });

        var boxes = new List<(CheckBox Box, CfgOption Opt)>();
        int y = 124;
        foreach (var o in p.Options)
        {
            var cb = new CheckBox { Text = o.Label, Location = new Point(12, y), AutoSize = true };
            boxes.Add((cb, o));
            page.Controls.Add(cb);
            y += 28;
        }
        var save = new Button { Text = "Save settings", Location = new Point(12, y + 8), Size = new Size(130, 30) };
        if (p.Options.Length > 0) page.Controls.Add(save);

        install.Click += (_, _) => Do(() => Installer.Install(HsDir, p), $"Installed {p.Title}. Start Hearthstone to load it.", true);
        uninstall.Click += (_, _) => Do(() => Installer.Uninstall(HsDir, p), $"Removed {p.Title}.", true);
        save.Click += (_, _) => Do(() =>
        {
            var path = Cfg.PathFor(HsDir, p.Cfg!);
            foreach (var (box, opt) in boxes)
                Cfg.Set(path, opt.Section, opt.Key, box.Checked ? "true" : "false");
        }, "Saved. Restart Hearthstone to apply.", false);

        _refreshers.Add(() =>
        {
            bool installed = Installer.IsInstalled(HsDir, p);
            bool cfgExists = p.Cfg is null || File.Exists(Cfg.PathFor(HsDir, p.Cfg));
            state.Text = installed ? "Installed" : "Not installed";
            if (!cfgExists) state.Text += " - start Hearthstone once to create its settings";
            install.Enabled = !installed;
            uninstall.Enabled = installed;
            foreach (var (box, opt) in boxes)
            {
                box.Enabled = cfgExists;
                if (cfgExists) box.Checked = Cfg.Get(Cfg.PathFor(HsDir, p.Cfg!), opt.Section, opt.Key) == "true";
            }
            save.Enabled = cfgExists && boxes.Count > 0;
        });

        return page;
    }

    void Do(Action act, string ok, bool needHsClosed)
    {
        try
        {
            if (needHsClosed && Installer.HearthstoneRunning())
            {
                _status.Text = "Close Hearthstone first, then try again.";
                return;
            }
            act();
            _status.Text = ok;
        }
        catch (Exception ex)
        {
            _status.Text = "Failed: " + ex.Message;
        }
        RefreshAll();
    }

    void RefreshAll()
    {
        _running.Text = Installer.HearthstoneRunning()
            ? "Hearthstone is running. Close it before installing or removing mods."
            : "Hearthstone is not running.";
        foreach (var r in _refreshers) r();
    }
}
