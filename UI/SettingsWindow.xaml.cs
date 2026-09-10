using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NomisKitchenHDT.UI
{
    public partial class SettingsWindow : Window
    {
        readonly PluginConfig _config;
        readonly Services.UpdateChecker _updater;
        bool _loading = true;

        public event Action StyleApplied;

        public SettingsWindow(PluginConfig config, Services.UpdateChecker updater)
        {
            InitializeComponent();
            _config = config;
            _updater = updater;

            foreach (var f in System.Windows.Media.Fonts.SystemFontFamilies.Select(x => x.Source).OrderBy(x => x))
                FontCombo.Items.Add(f);

            LoadFromConfig();

            AutoUpdateCheck.IsChecked = _config.AutoCheckUpdates;
            AutoUpdateCheck.Checked += (_, _1) => { _config.AutoCheckUpdates = true; _config.Save(); };
            AutoUpdateCheck.Unchecked += (_, _1) => { _config.AutoCheckUpdates = false; _config.Save(); };
            if (_updater != null && _updater.UpdateAvailable) ShowUpdateAvailable();

            DisableAbbreviationCheck.Checked += (_, _1) => { _config.DisableAbbreviation = true; UpdateStatus(); };
            DisableAbbreviationCheck.Unchecked += (_, _1) => { _config.DisableAbbreviation = false; UpdateStatus(); };
            ShowApmCheck.Checked += (_, _1) => _config.ShowApmOverlay = true;
            ShowApmCheck.Unchecked += (_, _1) => _config.ShowApmOverlay = false;
            LockOverlayCheck.Checked += (_, _1) => { _config.LockOverlay = true; StyleApplied?.Invoke(); };
            LockOverlayCheck.Unchecked += (_, _1) => { _config.LockOverlay = false; StyleApplied?.Invoke(); };

            FontCombo.SelectionChanged += (_, _1) => OnLiveChange(null, null);
            BackgroundField.ColorChanged += OnColorChanged;
            BorderField.ColorChanged += OnColorChanged;
            LabelColorField.ColorChanged += OnColorChanged;
            ValueColorField.ColorChanged += OnColorChanged;
        }

        void LoadFromConfig()
        {
            var wasLoading = _loading;
            _loading = true;
            DisableAbbreviationCheck.IsChecked = _config.DisableAbbreviation;
            ShowApmCheck.IsChecked = _config.ShowApmOverlay;
            LockOverlayCheck.IsChecked = _config.LockOverlay;
            FontCombo.SelectedItem = _config.FontFamily;
            if (FontCombo.SelectedItem == null && FontCombo.Items.Count > 0)
                FontCombo.SelectedIndex = 0;
            LabelSizeSlider.Value = _config.LabelFontSize;
            ValueSizeSlider.Value = _config.ValueFontSize;
            ScaleSlider.Value = _config.OverlayScale;
            DecimalSlider.Value = _config.DecimalPlaces;
            LabelSizeValue.Text = ((int)_config.LabelFontSize).ToString();
            ValueSizeValue.Text = ((int)_config.ValueFontSize).ToString();
            ScaleValue.Text = _config.OverlayScale.ToString("F2", CultureInfo.InvariantCulture);
            DecimalValue.Text = _config.DecimalPlaces.ToString();
            BackgroundField.Hex = _config.BackgroundColor;
            BorderField.Hex = _config.BorderColor;
            LabelColorField.Hex = _config.LabelColor;
            ValueColorField.Hex = _config.ValueColor;
            UpdateStatus();
            _loading = false;
        }

        void UpdateStatus()
        {
            StatusText.Text = _config.DisableAbbreviation
                ? "Abbreviation off. Restart Hearthstone to apply."
                : "Abbreviation on (default 1.2k style).";
        }

        void OnLiveChange(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_loading) return;
            LabelSizeValue.Text = ((int)LabelSizeSlider.Value).ToString();
            ValueSizeValue.Text = ((int)ValueSizeSlider.Value).ToString();
            ScaleValue.Text = ScaleSlider.Value.ToString("F2", CultureInfo.InvariantCulture);
            DecimalValue.Text = ((int)DecimalSlider.Value).ToString();
            Commit();
            StyleApplied?.Invoke();
        }

        void OnColorChanged()
        {
            if (_loading) return;
            Commit();
            StyleApplied?.Invoke();
        }

        void Commit()
        {
            _config.FontFamily = FontCombo.SelectedItem as string ?? _config.FontFamily;
            _config.LabelFontSize = LabelSizeSlider.Value;
            _config.ValueFontSize = ValueSizeSlider.Value;
            _config.OverlayScale = ScaleSlider.Value;
            _config.DecimalPlaces = (int)DecimalSlider.Value;
            _config.BackgroundColor = BackgroundField.Hex;
            _config.BorderColor = BorderField.Hex;
            _config.LabelColor = LabelColorField.Hex;
            _config.ValueColor = ValueColorField.Hex;
        }

        void OnReset(object sender, RoutedEventArgs e)
        {
            var def = new PluginConfig();
            _config.FontFamily = def.FontFamily;
            _config.LabelFontSize = def.LabelFontSize;
            _config.ValueFontSize = def.ValueFontSize;
            _config.OverlayScale = def.OverlayScale;
            _config.DecimalPlaces = def.DecimalPlaces;
            _config.BackgroundColor = def.BackgroundColor;
            _config.BorderColor = def.BorderColor;
            _config.LabelColor = def.LabelColor;
            _config.ValueColor = def.ValueColor;
            LoadFromConfig();
            _config.Save();
            StyleApplied?.Invoke();
        }

        void OnClose(object sender, RoutedEventArgs e)
        {
            Commit();
            _config.Save();
            StyleApplied?.Invoke();
            Close();
        }

        void OnOpenLog(object sender, RoutedEventArgs e)
        {
            try
            {
                var p = Utils.Log.FilePath;
                if (!File.Exists(p)) File.WriteAllText(p, "");
                System.Diagnostics.Process.Start("notepad.exe", "\"" + p + "\"");
            }
            catch { }
        }

        async void OnCheckUpdate(object sender, RoutedEventArgs e)
        {
            if (_updater == null) return;
            UpdateStatusText.Text = "Checking...";
            CheckUpdateButton.IsEnabled = false;
            var available = await _updater.CheckAsync();
            CheckUpdateButton.IsEnabled = true;
            if (available) ShowUpdateAvailable();
            else UpdateStatusText.Text = "Up to date.";
        }

        void ShowUpdateAvailable()
        {
            UpdateStatusText.Text = "Update available: v" + _updater.LatestVersion;
            UpdateButton.Visibility = Visibility.Visible;
        }

        async void OnDownloadUpdate(object sender, RoutedEventArgs e)
        {
            if (_updater == null) return;
            UpdateButton.IsEnabled = false;
            UpdateStatusText.Text = "Downloading...";
            var ok = await _updater.DownloadAndRunAsync();
            if (ok)
                UpdateStatusText.Text = "Installer launched. Close HDT when it asks.";
            else
            {
                UpdateStatusText.Text = "Download failed. Opening release page.";
                try { System.Diagnostics.Process.Start(_updater.ReleaseUrl ?? "https://github.com/RainWritesCode/NomisKitchenHDT/releases"); } catch { }
            }
            UpdateButton.IsEnabled = true;
        }
    }
}
