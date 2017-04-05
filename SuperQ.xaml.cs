using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    /// Interaction logic for SuperQ.xaml
    /// </summary>
    public partial class SuperQ : UserControl, INotifyPropertyChanged
    {
        private double _draggerXOffset = 168;
        private double _draggerYOffset = 59;
        private double _draggerEyeOffset = -4;

        public SuperQ()
        {
            InitializeComponent();
            Reset();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private double _bandwidth = 0.8;

        private double[] _loudness = new double[] { 1000d, 0.8, default(double) };
        public double[] Loudness
        {
            get => _loudness;
            set
            {
                _loudness = value;
                OnPropertyChanged("Loudness");
            }
        }

        private void Reset()
        {
            var dragMargin = DraggerEye.Margin;
            DraggerEye.Margin = new Thickness(164,55,0,0);
            var dragXMargin = DraggerVert.Margin;
            dragXMargin.Left = _draggerXOffset;
            DraggerVert.Margin = dragXMargin;
            var dragYMargin = DraggerHorz.Margin;
            dragYMargin.Top = _draggerYOffset;
            DraggerHorz.Margin = dragYMargin;
            Slider_Bandwidth.Value = 0.8;
            ResetEQ();
        }

        private void ResetEQ()
        {
            double freq = 0;
            double volume = 0;
            Hertz.Content = $"Frequency: {freq:0}kHz";
            BoostCut.Content = $"Gain: {volume:0}dB";
            Loudness = new double[] { freq, _bandwidth, volume };
        }


        private SuperQ _activated = null;

        private void ManipulationPad_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _activated = this;
            this.ResetEQ();
        }

        private void ManipulationPad_MouseMove(object sender, MouseEventArgs e)
        {
            if (_activated == this && Mouse.LeftButton == MouseButtonState.Pressed)
            {
                var point = e.GetPosition(ManipulationPad);
                var xUnit = 100 / ManipulationPad.ActualWidth;
                var yUnit = 70 / ManipulationPad.ActualHeight;
                var midY = ManipulationPad.ActualHeight / 2;

                var dragMargin = DraggerEye.Margin;
                dragMargin.Left = point.X + _draggerEyeOffset;
                dragMargin.Top = point.Y + _draggerEyeOffset;
                DraggerEye.Margin = dragMargin;
                var dragXMargin = DraggerVert.Margin;
                dragXMargin.Left = point.X;
                DraggerVert.Margin = dragXMargin;
                var dragYMargin = DraggerHorz.Margin;
                dragYMargin.Top = point.Y;
                DraggerHorz.Margin = dragYMargin;

                var volume = 0d;
                if (point.Y >= midY)
                {
                    // this will be a negative number
                    volume = ((yUnit * point.Y) - 35) * -1;
                }
                else
                {
                    volume = 35 - (yUnit * point.Y);
                }

                // use a linear scale for the frequencies
                //double freq = (point.X * xUnit / 100) * 10000;

                //create a logarithmic scale for the frequenies

                double freq = 1000;
                var val = ManipulationPad.ActualWidth - point.X;
                if (val <= 1)
                {
                    freq = 1000; // log is undefined for 0, log(1) = 0
                }
                else
                {
                    freq = 10000 - (((100 * Math.Log(val) / Math.Log(ManipulationPad.ActualWidth)) / 100) * 10000);
                }

                //Debug.WriteLine($"Frequency: {freq}, Volume: {volume}");
                Hertz.Content = $"Frequency: {freq:0}kHz";
                BoostCut.Content = $"Gain: {volume:0}dB";
                Loudness = new double[] { freq, _bandwidth, volume };
            }
        }

        private void ManipulationPad_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine($"Mouse Up  : {e.GetPosition(ManipulationPad)}");
            _activated = null;
        }

        private void Slider_Bandwidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _bandwidth = e.NewValue;
            Debug.WriteLine($"Value of Bandwidth: {e.NewValue}");
        }

        private void ResetSuperQButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Reset();
        }
    }
}
