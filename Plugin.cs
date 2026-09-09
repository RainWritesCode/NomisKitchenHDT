using System;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Plugins;
using NomisKitchenHDT.Services;
using NomisKitchenHDT.UI;

namespace NomisKitchenHDT
{
    public class Plugin : IPlugin
    {
        public string Name => "Nomi's Kitchen";
        public string Description => "Battlegrounds QoL: disable board number abbreviation, live APM overlay.";
        public string Author => "RainWritesCode";
        public string ButtonText => "Settings";
        public Version Version => new Version(1, 0, 0);
        public MenuItem MenuItem => _menuItem;

        MenuItem _menuItem;
        internal PluginConfig Config;
        internal ApmTracker Tracker;
        internal ApmOverlay Overlay;
        internal AbbreviationDisabler Disabler;
        internal ApmProviderInstaller ApmInstaller;

        internal static Plugin Instance;

        public void OnLoad()
        {
            Instance = this;
            Config = PluginConfig.Load();

            Disabler = new AbbreviationDisabler(Config);
            Disabler.SyncWithSetting();

            ApmInstaller = new ApmProviderInstaller(Config);
            ApmInstaller.EnsureInstalled();

            Tracker = new ApmTracker();
            Tracker.Start();

            Overlay = new ApmOverlay(Config, Tracker);
            if (Config.ShowApmOverlay)
                Overlay.Show();

            _menuItem = new MenuItem { Header = "Nomi's Kitchen" };
            _menuItem.Click += (_, _1) => OpenSettings();
        }

        public void OnUnload()
        {
            Overlay?.Hide();
            Tracker?.Stop();
            Disabler?.Dispose();
            Config?.Save();
        }

        public void OnUpdate() { }
        public void OnButtonPress() => OpenSettings();

        void OpenSettings()
        {
            var win = new SettingsWindow(Config);
            win.Closed += (_, _1) =>
            {
                Config.Save();
                Disabler.SyncWithSetting();
                if (Config.ShowApmOverlay) Overlay.Show(); else Overlay.Hide();
            };
            win.Show();
        }
    }
}
