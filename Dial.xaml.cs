using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Media;

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for Dial.xaml
    /// </summary>
    public partial class Dial : UserControl, INotifyPropertyChanged
    {
        public Dial()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static readonly DependencyProperty MaximumGainProperty =
            DependencyProperty.Register(
                "MaximumGain", 
                typeof(double), typeof(Dial), 
                new PropertyMetadata(default(double)));
        
        public double MaximumGain
        {
            get => (double)GetValue(MaximumGainProperty);
            set => SetValue(MaximumGainProperty, value);
        }

        public static readonly DependencyProperty MinimumGainProperty =
            DependencyProperty.Register(
                "MinimumGain", 
                typeof(double), typeof(Dial),
                new PropertyMetadata(default(double)));
        
        public double MinimumGain
        {
            get => (double)GetValue(MinimumGainProperty);
            set => SetValue(MinimumGainProperty, value);
        }

        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register(
                "Angle", 
                typeof(double), typeof(Dial),
                new PropertyMetadata(default(double)));
        
        public double Angle
        {
            get => (double)GetValue(AngleProperty);
            set => SetValue(AngleProperty, value);
        }

        private double _amount = default(double);
        public double Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged("Amount");
            }
        }

        private Dial _activated = null;

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _activated = this;
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _activated = null;
        }

        private double _maxAngle = 135; // equals +30 in boost
        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            var mover = e.Source as Dial;

            if (_activated == this && Mouse.LeftButton == MouseButtonState.Pressed)
            {
                var angle = GetAngle(Mouse.GetPosition(this), this.RenderSize);

                // ensure angle is in the cut or boost range
                if (angle >= 360 - _maxAngle || angle <= _maxAngle)
                {
                    // make a center detent
                    if (angle <= 12 || angle >= 348) { angle = 0; }
                    Angle = angle;
                    ConvertAngleToAmount();
                }
            }
        }

        public enum Quadrants : int { nw = 2, ne = 1, sw = 4, se = 3 };
        private double GetAngle(Point point, Size renderSize)
        {
            var x = point.X - (renderSize.Width / 2d);
            var y = renderSize.Height - point.Y - (renderSize.Height / 2d);

            var hypot = Math.Sqrt(x * x + y * y);
            var val = Math.Asin(y / hypot) * 180 / Math.PI;

            var quadrant = x >= 0 ?
                y >= 0 ? Quadrants.ne : Quadrants.se :
                y >= 0 ? Quadrants.nw : Quadrants.sw;

            switch (quadrant)
            {
                case Quadrants.ne: val = 90 - val; break;
                case Quadrants.nw: val = 270 + val; break;
                case Quadrants.se: val = 90 - val; break;
                case Quadrants.sw: val = 270 + val; break;
            }

            return val;
        }

        private void ConvertAngleToAmount()
        {
            var degree = Angle;
            if (degree > 180)
            {
                // convert to negative for cut, else boost
                degree = (360 - degree) * -1;
            }
            Amount = MaximumGain / _maxAngle * degree;
        }
    }
}
