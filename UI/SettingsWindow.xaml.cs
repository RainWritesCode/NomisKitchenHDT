using System.Windows;

namespace NomisKitchenHDT.UI
{
    public partial class SettingsWindow : Window
    {
        private readonly PluginConfig _config;

        public SettingsWindow(PluginConfig config)
        {
            InitializeComponent();
            _config = config;
            DisableAbbreviationCheck.IsChecked = _config.DisableAbbreviation;
            ShowApmCheck.IsChecked = _config.ShowApmOverlay;

            DisableAbbreviationCheck.Checked += (_, _1) => { _config.DisableAbbreviation = true; UpdateStatus(); };
            DisableAbbreviationCheck.Unchecked += (_, _1) => { _config.DisableAbbreviation = false; UpdateStatus(); };
            ShowApmCheck.Checked += (_, _1) => _config.ShowApmOverlay = true;
            ShowApmCheck.Unchecked += (_, _1) => _config.ShowApmOverlay = false;

            UpdateStatus();
        }

        private void UpdateStatus()
        {
            StatusText.Text = _config.DisableAbbreviation
                ? "Abbreviation off. Restart Hearthstone to apply."
                : "Abbreviation on (default 1.2k style).";
        }

        private void OnClose(object sender, RoutedEventArgs e) => Close();
    }
}
