using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Hearthstone_Deck_Tracker;

namespace NomisKitchenHDT.UI
{
    public partial class ApmOverlay : UserControl
    {
        private readonly PluginConfig _config;
        private readonly Services.ApmTracker _tracker;
        private bool _visible;
        private bool _dragging;
        private Point _dragOffset;

        public ApmOverlay(PluginConfig config, Services.ApmTracker tracker)
        {
            InitializeComponent();
            _config = config;
            _tracker = tracker;
            _tracker.OnStatsUpdated += UpdateFromTracker;

            Canvas.SetLeft(this, _config.OverlayX);
            Canvas.SetTop(this, _config.OverlayY);

            MouseLeftButtonDown += OnMouseDown;
            MouseMove += OnMouseMove;
            MouseLeftButtonUp += OnMouseUp;
        }

        private static Canvas GetOverlayCanvas()
        {
            var overlay = Core.Overlay;
            if (overlay == null) return null;
            var field = overlay.GetType().GetField("CanvasInfo",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return field?.GetValue(overlay) as Canvas;
        }

        public void Show()
        {
            if (_visible) return;
            var canvas = GetOverlayCanvas();
            if (canvas == null) return;
            if (!canvas.Children.Contains(this))
                canvas.Children.Add(this);
            _visible = true;
        }

        public void Hide()
        {
            if (!_visible) return;
            var canvas = GetOverlayCanvas();
            if (canvas != null && canvas.Children.Contains(this))
                canvas.Children.Remove(this);
            _visible = false;
        }

        private void UpdateFromTracker()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ActionsText.Text = _tracker.ActionsThisTurn.ToString();
                PeakText.Text = _tracker.PeakApm.ToString("F1");
                AverageText.Text = _tracker.AverageApm.ToString("F1");
            }));
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _dragging = true;
            _dragOffset = e.GetPosition(this);
            CaptureMouse();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging) return;
            var canvas = GetOverlayCanvas();
            if (canvas == null) return;
            var pos = e.GetPosition(canvas);
            Canvas.SetLeft(this, pos.X - _dragOffset.X);
            Canvas.SetTop(this, pos.Y - _dragOffset.Y);
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_dragging) return;
            _dragging = false;
            ReleaseMouseCapture();
            _config.OverlayX = Canvas.GetLeft(this);
            _config.OverlayY = Canvas.GetTop(this);
            _config.Save();
        }
    }
}
