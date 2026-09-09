using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace NomisKitchenHDT.UI
{
    public partial class ApmOverlay : Window
    {
        const int GWL_EXSTYLE = -20;
        const int WS_EX_TRANSPARENT = 0x20;
        const int WS_EX_LAYERED = 0x80000;

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        readonly PluginConfig _config;
        readonly Services.ApmTracker _tracker;
        bool _shown;

        public ApmOverlay(PluginConfig config, Services.ApmTracker tracker)
        {
            InitializeComponent();
            _config = config;
            _tracker = tracker;
            _tracker.OnStatsUpdated += UpdateFromTracker;

            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = _config.OverlayX;
            Top = _config.OverlayY;

            MouseLeftButtonDown += OnDragStart;
            LocationChanged += OnLocationChanged;
            SourceInitialized += (_, _1) => ApplyLock();

            ApplyStyle();
        }

        public new void Show()
        {
            if (_shown) return;
            base.Show();
            _shown = true;
            ApplyLock();
        }

        public new void Hide()
        {
            if (!_shown) return;
            base.Hide();
            _shown = false;
        }

        public void ApplyStyle()
        {
            try
            {
                var font = new FontFamily(string.IsNullOrEmpty(_config.FontFamily) ? "Segoe UI" : _config.FontFamily);
                var labelBrush = BrushFromHex(_config.LabelColor, "#B8B8B8");
                var valueBrush = BrushFromHex(_config.ValueColor, "#F5A623");
                var bgBrush = BrushFromHex(_config.BackgroundColor, "#EE1A1A1A");
                var borderBrush = BrushFromHex(_config.BorderColor, "#33FFFFFF");

                RootBorder.Background = bgBrush;
                RootBorder.BorderBrush = borderBrush;
                RootBorder.LayoutTransform = new ScaleTransform(_config.OverlayScale, _config.OverlayScale);

                foreach (var lbl in new[] { LabelActions, LabelPeak, LabelAverage })
                {
                    lbl.FontFamily = font;
                    lbl.FontSize = _config.LabelFontSize;
                    lbl.Foreground = labelBrush;
                }
                foreach (var v in new[] { ActionsText, PeakText, AverageText })
                {
                    v.FontFamily = font;
                    v.FontSize = _config.ValueFontSize;
                    v.Foreground = valueBrush;
                }

                RootBorder.Cursor = _config.LockOverlay ? Cursors.Arrow : Cursors.SizeAll;
                ApplyLock();
            }
            catch { }
        }

        void ApplyLock()
        {
            try
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                if (hwnd == IntPtr.Zero) return;
                int ex = GetWindowLong(hwnd, GWL_EXSTYLE);
                if (_config.LockOverlay) ex |= WS_EX_TRANSPARENT | WS_EX_LAYERED;
                else ex &= ~WS_EX_TRANSPARENT;
                SetWindowLong(hwnd, GWL_EXSTYLE, ex);
            }
            catch { }
        }

        void UpdateFromTracker()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var fmt = "F" + Math.Max(0, Math.Min(3, _config.DecimalPlaces));
                ActionsText.Text = _tracker.ActionsThisTurn.ToString(CultureInfo.InvariantCulture);
                PeakText.Text = _tracker.PeakApm.ToString(fmt, CultureInfo.InvariantCulture);
                AverageText.Text = _tracker.AverageApm.ToString(fmt, CultureInfo.InvariantCulture);
            }));
        }

        void OnDragStart(object sender, MouseButtonEventArgs e)
        {
            if (_config.LockOverlay) return;
            try { DragMove(); } catch { }
        }

        void OnLocationChanged(object sender, EventArgs e)
        {
            _config.OverlayX = Left;
            _config.OverlayY = Top;
        }

        static SolidColorBrush BrushFromHex(string hex, string fallback)
        {
            try { return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)); }
            catch { return new SolidColorBrush((Color)ColorConverter.ConvertFromString(fallback)); }
        }
    }
}
