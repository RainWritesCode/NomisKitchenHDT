using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NomisKitchenHDT.UI
{
    public partial class ColorField : UserControl
    {
        Color _color = Colors.Black;
        bool _suppress;

        public event Action ColorChanged;

        public ColorField()
        {
            InitializeComponent();
        }

        public string Hex
        {
            get => "#" + _color.A.ToString("X2") + _color.R.ToString("X2") + _color.G.ToString("X2") + _color.B.ToString("X2");
            set => TrySetHex(value);
        }

        void TrySetHex(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return;
            try
            {
                var c = (Color)ColorConverter.ConvertFromString(s.Trim());
                _color = c;
                Refresh();
            }
            catch { }
        }

        void Refresh()
        {
            _suppress = true;
            Swatch.Background = new SolidColorBrush(_color);
            AlphaSlider.Value = _color.A;
            HexBox.Text = Hex;
            _suppress = false;
        }

        void AlphaSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_suppress) return;
            _color = Color.FromArgb((byte)e.NewValue, _color.R, _color.G, _color.B);
            _suppress = true;
            Swatch.Background = new SolidColorBrush(_color);
            HexBox.Text = Hex;
            _suppress = false;
            ColorChanged?.Invoke();
        }

        void Transparent_Click(object sender, RoutedEventArgs e)
        {
            _color = Color.FromArgb(0, _color.R, _color.G, _color.B);
            Refresh();
            ColorChanged?.Invoke();
        }

        void Swatch_Click(object sender, MouseButtonEventArgs e)
        {
            using var dlg = new System.Windows.Forms.ColorDialog
            {
                Color = System.Drawing.Color.FromArgb(_color.R, _color.G, _color.B),
                FullOpen = true,
                AnyColor = true,
                SolidColorOnly = false,
            };
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _color = Color.FromArgb(_color.A, dlg.Color.R, dlg.Color.G, dlg.Color.B);
                Refresh();
                ColorChanged?.Invoke();
            }
        }

        void HexBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_suppress) return;
            TrySetHex(HexBox.Text);
            ColorChanged?.Invoke();
        }

        void HexBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { TrySetHex(HexBox.Text); ColorChanged?.Invoke(); }
        }
    }
}
