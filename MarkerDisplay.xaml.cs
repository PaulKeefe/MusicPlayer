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
    /// Interaction logic for MarkerDisplay.xaml
    /// </summary>
    public partial class MarkerDisplay : UserControl
    {
        public MarkerDisplay()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty MarkersProperty =
            DependencyProperty.Register("Markers", typeof(List<Marker>), typeof(MarkerDisplay),
                new PropertyMetadata(new PropertyChangedCallback(MarkersPropertyChanged)));

        public List<Marker> Markers
        {
            get => (List<Marker>)GetValue(MarkersProperty);
            set => SetValue(MarkersProperty, value);
        }

        public static void MarkersPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MarkerDisplay;
            if (control != null)
            {
                control.MarkerView.Children.Clear();

                var markers = e.NewValue as List<Marker>;
                foreach (var marker in markers)
                {
                    var tMarker = new TimeMarker();
                    tMarker.Time = marker.Time;
                    tMarker.Distance = marker.Offset - (tMarker.Width / 2);
                    tMarker.Height = 100; // temp
                    control.MarkerView.Children.Add(tMarker);
                }
            }
        }
    }
}
