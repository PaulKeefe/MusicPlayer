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
using System.Diagnostics;

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for stereoChannelControl.xaml
    /// </summary>
    public partial class StereoChannelControl : UserControl
    {
        public StereoChannelControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty VolumeProperty =
            DependencyProperty.Register("Volume", typeof(double), typeof(StereoChannelControl),
                new PropertyMetadata(new PropertyChangedCallback(stereoChannelControlPropertyChanged)));

        public double Volume
        {
            get => (double)GetValue(VolumeProperty);
            set => SetValue(VolumeProperty, value);
        }

        public static void stereoChannelControlPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as StereoChannelControl;
            if (control != null)
            {
                control.channelBar.Value = (double)e.NewValue;
                control.channelOverload.Visibility = (double)e.NewValue >= 100 ? Visibility.Visible : Visibility.Hidden;
            }
        }
    }
}
