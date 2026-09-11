using System;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Plugins;
using NomisKitchenHDT.Services;
using NomisKitchenHDT.UI;
using NomisKitchenHDT.Utils;

namespace NomisKitchenHDT
{
    public class Plugin : IPlugin
    {
        public string Name => "Nomi's Kitchen";
        public string Description => "Battlegrounds QoL: disable board number abbreviation, live APM overlay.";
        public string Author => "RainWritesCode";
        public string ButtonText => "Settings";
        public Version Version => new Version(1, 0, 4);
        public MenuItem MenuItem => _menuItem;

        MenuItem _menuItem;
        internal PluginConfig Config;
        internal ApmTracker Tracker;
        internal ApmOverlay Overlay;
        internal AbbreviationDisabler Disabler;
        internal ApmProviderInstaller ApmInstaller;
        internal UpdateChecker Updater;

        internal static Plugin Instance;

        public void OnLoad()
        {
            Instance = this;
            Config = PluginConfig.Load();
            Log.Info("=== Nomi's Kitchen v" + Version + " loading ===");
            Log.Info("Config: HearthstoneDir='" + Config.HearthstoneDir + "' ShowApmOverlay=" + Config.ShowApmOverlay + " DisableAbbreviation=" + Config.DisableAbbreviation + " LockOverlay=" + Config.LockOverlay + " AutoCheckUpdates=" + Config.AutoCheckUpdates + " Log=" + Log.FilePath);

            Disabler = new AbbreviationDisabler(Config);
            Disabler.SyncWithSetting();

            ApmInstaller = new ApmProviderInstaller(Config);
            ApmInstaller.EnsureInstalled();

            Tracker = new ApmTracker();
            Tracker.Start();

            Overlay = new ApmOverlay(Config, Tracker);
            Log.Info("Overlay created; ShowApmOverlay=" + Config.ShowApmOverlay);
            if (Config.ShowApmOverlay)
                Overlay.Show();

            _menuItem = new MenuItem { Header = "Nomi's Kitchen" };
            _menuItem.Click += (_, _1) => OpenSettings();

            Updater = new UpdateChecker(Version);
            if (Config.AutoCheckUpdates)
                _ = CheckAndPromptAsync();
        }

        async System.Threading.Tasks.Task CheckAndPromptAsync()
        {
            var available = await Updater.CheckAsync();
            Log.Info(available ? ("Update available: v" + Updater.LatestVersion) : "Update check: up to date, or check failed");
            if (!available) return;
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher == null) return;
            dispatcher.BeginInvoke(new Action(async () =>
            {
                var result = System.Windows.MessageBox.Show(
                    $"Nomi's Kitchen v{Updater.LatestVersion} is available (you have v{Version}).\n\nDownload and install now? Hearthstone Deck Tracker will close so the update can apply.",
                    "Nomi's Kitchen update",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Information);
                if (result != System.Windows.MessageBoxResult.Yes) return;
                var ok = await Updater.DownloadAndRunAsync();
                if (ok) System.Windows.Application.Current.Shutdown();
                else System.Windows.MessageBox.Show("Update download failed. Try the button in Nomi's Kitchen settings, or grab it from the Releases page.",
                    "Nomi's Kitchen", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }));
        }

        public void OnUnload()
        {
            Overlay?.Hide();
            Log.Info("Nomi's Kitchen unloading"); Tracker?.Stop();
            Disabler?.Dispose();
            Config?.Save();
        }

        public void OnUpdate() { }
        public void OnButtonPress() => OpenSettings();

        void OpenSettings()
        {
            Log.Info("Settings opened");
            var win = new SettingsWindow(Config, Updater);
            win.StyleApplied += () => Overlay?.ApplyStyle();
            win.Closed += (_, _1) =>
            {
                Config.Save();
                Disabler.SyncWithSetting();
                if (Config.ShowApmOverlay) Overlay.Show(); else Overlay.Hide();
                Overlay?.ApplyStyle();
            };
            win.Show();
        }
    }
}
