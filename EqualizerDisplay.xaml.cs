using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for EqualizerDisplay.xaml
    /// </summary>
    public partial class EqualizerDisplay : UserControl
    {
        private FrequencyBar[] frequencyBars;

        public EqualizerDisplay()
        {
            InitializeComponent();
            Prepare();
        }

        public static readonly DependencyProperty MinimumDbLevelProperty =
            DependencyProperty.Register("MinimumDbLevel", typeof(double), typeof(EqualizerDisplay),
                new PropertyMetadata(-60d, new PropertyChangedCallback(MinimumDbLevelUpdated)));

        public double MinimumDbLevel
        {
            get => (double)GetValue(MinimumDbLevelProperty);
            set => SetValue(MinimumDbLevelProperty, value);
        }

        private static void MinimumDbLevelUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as EqualizerDisplay).UpdateDbDisplay((double)e.NewValue);
        }

        private void UpdateDbDisplay(double value)
        {
            Debug.WriteLine($"Value of dB: {value}");
            LowestDbLabel.Content = value.ToString() + "dB";
        }

        public static readonly DependencyProperty MagnitudesProperty =
            DependencyProperty.Register("Magnitudes", typeof(double[]), typeof(EqualizerDisplay),
                new PropertyMetadata(new PropertyChangedCallback(MagnitudesUpdated)));

        public double[] Magnitudes
        {
            get => (double[])GetValue(MagnitudesProperty);
            set => SetValue(MagnitudesProperty, value);
        }

        private static void MagnitudesUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as EqualizerDisplay).UpdateMagnitudes((double[])e.NewValue);
        }

        private void UpdateMagnitudes(double[] Mags)
        {
            for (var i = 0; i < Mags.Length; i++)
            {
                var intensityDB = Mags[i];

                if (intensityDB < MinimumDbLevel) intensityDB = MinimumDbLevel;

                // percent with -60 = 1
                double percent = intensityDB / MinimumDbLevel;

                // invert the percent using height of the bar element
                var barHeight = spec0.ActualHeight - (percent * spec0.ActualHeight);
                //var barHeight = _maximumFreqBarHeight - (percent * _maximumFreqBarHeight);

                // set height of control
                frequencyBars[i].Height = barHeight > 2 ? barHeight : 2;

                //Debug.WriteLine($"Intensity: {intensityDB}, Percent: {percent}");
            }
        }

        private void Prepare()
        {
            frequencyBars = new FrequencyBar[]
            {
                new FrequencyBar(spec1, peak1),
                new FrequencyBar(spec2, peak2),
                new FrequencyBar(spec3, peak3),
                new FrequencyBar(spec4, peak4),
                new FrequencyBar(spec5, peak5),
                new FrequencyBar(spec6, peak6),
                new FrequencyBar(spec7, peak7),
                new FrequencyBar(spec8, peak8),
                new FrequencyBar(spec9, peak9),
                new FrequencyBar(spec10, peak10),
                new FrequencyBar(spec11, peak11),
                new FrequencyBar(spec12, peak12),
                new FrequencyBar(spec13, peak13),
                new FrequencyBar(spec14, peak14),
                new FrequencyBar(spec15, peak15),
                new FrequencyBar(spec16, peak16),
                new FrequencyBar(spec17, peak17),
                new FrequencyBar(spec18, peak18),
                new FrequencyBar(spec19, peak19)
            };
        }


        class FrequencyBar
        {
            private bool peakFalling = false;
            private double lastPeakPosition = 0;
            private DispatcherTimer peakTimer;
            private DispatcherTimer fallTimer;

            public FrequencyBar(Rectangle bar, Rectangle peak)
            {
                this.Bar = bar;
                this.Peak = peak;
                this.Bar.Height = 2;
                this.Peak.Height = 2;

                peakTimer = new DispatcherTimer();
                peakTimer.Interval = TimeSpan.FromMilliseconds(1000);
                peakTimer.Tick += Peak_Tick;

                fallTimer = new DispatcherTimer();
                fallTimer.Interval = TimeSpan.FromMilliseconds(100);
                fallTimer.Tick += Fall_Tick;
            }

            private void Fall_Tick(object sender, EventArgs e)
            {
                if (Peak.Margin.Bottom > lastPeakPosition - 20)
                {
                    var margin = Peak.Margin;
                    margin.Bottom -= 2;
                    Peak.Margin = margin;
                    Peak.Opacity -= .2;
                }
                else
                {
                    fallTimer.Stop();
                    peakFalling = false;
                    var margin = Peak.Margin;
                    margin.Bottom = 0;
                    Peak.Margin = margin;
                }
            }

            private void Peak_Tick(object sender, EventArgs e)
            {
                peakTimer.Stop();
                if (!peakFalling)
                {
                    peakFalling = true;
                    lastPeakPosition = Peak.Margin.Bottom;
                    fallTimer.Start();
                }
            }

            public double Height
            {
                get => Bar.Height;
                set
                {
                    Bar.Height = value;
                    if (Bar.Height - 2 >= Peak.Margin.Bottom)
                    {
                        peakTimer.Stop();
                        fallTimer.Stop();
                        peakFalling = false;

                        var thickness = Peak.Margin;
                        thickness.Bottom = Bar.Height - 2;
                        Peak.Margin = thickness;
                        Peak.Opacity = 1;

                        peakTimer.Start();
                    }
                }
            }
            public Rectangle Bar { get; set; }
            public Rectangle Peak { get; set; }
        }
    }
}
