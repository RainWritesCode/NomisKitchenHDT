using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ApiCore = Hearthstone_Deck_Tracker.API.Core;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace NomisKitchenHDT.UI
{
    public partial class ApmOverlay : UserControl
    {
        readonly PluginConfig _config;
        readonly Services.ApmTracker _tracker;
        bool _visible;
        bool _dragging;
        Point _dragStart;
        double _startLeft, _startTop;

        public ApmOverlay(PluginConfig config, Services.ApmTracker tracker)
        {
            InitializeComponent();
            _config = config;
            _tracker = tracker;
            _tracker.OnStatsUpdated += UpdateFromTracker;

            MouseLeftButtonDown += OnDown;
            MouseLeftButtonUp += OnUp;
            MouseMove += OnMove;

            ApplyStyle();
        }

        public void Show()
        {
            if (_visible) return;
            var canvas = ApiCore.OverlayCanvas;
            if (canvas == null) return;
            if (!canvas.Children.Contains(this))
                canvas.Children.Add(this);
            Panel.SetZIndex(this, 10000);
            Canvas.SetLeft(this, _config.OverlayX);
            Canvas.SetTop(this, _config.OverlayY);
            OverlayExtensions.SetIsOverlayHitTestVisible(this, !_config.LockOverlay);
            _visible = true;
        }

        public void Hide()
        {
            if (!_visible) return;
            var canvas = ApiCore.OverlayCanvas;
            if (canvas != null && canvas.Children.Contains(this))
                canvas.Children.Remove(this);
            _visible = false;
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
                RootBorder.Cursor = _config.LockOverlay ? Cursors.Arrow : Cursors.SizeAll;

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

                OverlayExtensions.SetIsOverlayHitTestVisible(this, !_config.LockOverlay);
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

        void OnDown(object sender, MouseButtonEventArgs e)
        {
            if (_config.LockOverlay) return;
            var canvas = ApiCore.OverlayCanvas;
            if (canvas == null) return;
            _dragging = true;
            _dragStart = e.GetPosition(canvas);
            _startLeft = SafeCoord(Canvas.GetLeft(this), _config.OverlayX);
            _startTop = SafeCoord(Canvas.GetTop(this), _config.OverlayY);
            CaptureMouse();
            e.Handled = true;
        }

        void OnMove(object sender, MouseEventArgs e)
        {
            if (!_dragging) return;
            var canvas = ApiCore.OverlayCanvas;
            if (canvas == null) return;
            var p = e.GetPosition(canvas);
            double newLeft = _startLeft + (p.X - _dragStart.X);
            double newTop = _startTop + (p.Y - _dragStart.Y);

            double maxLeft = Math.Max(0, canvas.ActualWidth - ActualWidth);
            double maxTop = Math.Max(0, canvas.ActualHeight - ActualHeight);
            if (maxLeft > 0) newLeft = Math.Max(0, Math.Min(newLeft, maxLeft));
            if (maxTop > 0) newTop = Math.Max(0, Math.Min(newTop, maxTop));

            Canvas.SetLeft(this, newLeft);
            Canvas.SetTop(this, newTop);
        }

        void OnUp(object sender, MouseButtonEventArgs e)
        {
            if (!_dragging) return;
            _dragging = false;
            ReleaseMouseCapture();
            _config.OverlayX = SafeCoord(Canvas.GetLeft(this), _config.OverlayX);
            _config.OverlayY = SafeCoord(Canvas.GetTop(this), _config.OverlayY);
            _config.Save();
            e.Handled = true;
        }

        static double SafeCoord(double v, double fallback) => double.IsNaN(v) ? fallback : v;

        static SolidColorBrush BrushFromHex(string hex, string fallback)
        {
            try { return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)); }
            catch { return new SolidColorBrush((Color)ColorConverter.ConvertFromString(fallback)); }
        }
    }
}
