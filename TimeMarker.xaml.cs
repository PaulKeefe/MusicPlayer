using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for TimeMarker.xaml
    /// </summary>
    public partial class TimeMarker : UserControl
    {
        public TimeMarker()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(string), typeof(TimeMarker),
                new PropertyMetadata(new PropertyChangedCallback(TimePropertyChanged)));

        public string Time
        {
            get => (string)GetValue(TimeProperty);
            set => SetValue(TimeProperty, value);
        }

        public static readonly DependencyProperty DistanceProperty =
            DependencyProperty.Register("Distance", typeof(double), typeof(TimeMarker),
                new PropertyMetadata(new PropertyChangedCallback(DistancePropertyChanged)));

        public double Distance
        {
            get => (double)GetValue(DistanceProperty);
            set => SetValue(DistanceProperty, value);
        }

        public static void TimePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as TimeMarker;
            if (control != null)
            {
                control.TimeTop.Content = (string)e.NewValue;
                control.TimeBottom.Content = (string)e.NewValue;
            }
        }

        public static void DistancePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as TimeMarker;
            if (control != null)
            {
                var margin = control.Margin;
                margin.Left = (double)e.NewValue - (control.Width / 2);
                control.Margin = margin;
            }
        }
    }
}
