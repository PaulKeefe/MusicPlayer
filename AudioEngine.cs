using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.ComponentModel;

namespace MusicPlayer
{
    public class AudioEngine : INotifyPropertyChanged
    {
        // Declarations for audio out and MP3 stream
        WaveOut player;
        Equalizer equalizer;
        AudioFileReader reader;
        SampleAggregator aggregator;
        MeteringSampleProvider sampleProvider;

        int xPos = 2;
        int yScale = 50;
        
        List<Point> topPoints = new List<Point>();
        List<Point> bottomPoints = new List<Point>();
        
        private double _maxFreq = 0;
        //private double _minFreq = 0;
        private double[] _fftResult = { };
        private bool _songLoaded = false;
        private double[] _volume = { 0, 0 };

        private EqualizerBand[] _bands;

        private const int REQUIRED_SAMPLE_SIZE = 2048;


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public bool SongLoaded
        {
            get => _songLoaded;
            set
            {
                _songLoaded = value;
                OnPropertyChanged("SongLoaded");
            }
        }

        public int XPos
        {
            get => xPos;
            set => xPos = value;
        }

        public int YScale
        {
            get => yScale;
            set => yScale = value;
        }

        public Polygon WaveformPoly()
        {
            var polygon = new Polygon();
            polygon.Points.Add(new Point(0, yScale));

            foreach (var p in topPoints)
            {
                polygon.Points.Add(p);
            }

            polygon.Points.Add(new Point(xPos, yScale));
            bottomPoints.Reverse();

            foreach (var p in bottomPoints)
            {
                polygon.Points.Add(p);
            }
            polygon.Points.Add(new Point(0, yScale));

            polygon.Fill = (LinearGradientBrush)Application.Current.Resources["WaveformBrush"];
            polygon.FillRule = FillRule.EvenOdd;
            polygon.Stroke = Brushes.Black;
            polygon.StrokeThickness = 1;

            return polygon;
        }

        public PointCollection WaveformPoints
        {
            get
            {
                PointCollection points = new PointCollection();

                points.Add(new Point(0, yScale));

                foreach (var p in topPoints)
                {
                    points.Add(p);
                }

                points.Add(new Point(xPos, yScale));
                bottomPoints.Reverse();

                foreach (var p in bottomPoints)
                {
                    points.Add(p);
                }
                points.Add(new Point(0, yScale));

                return points;
            }
        }

        public MMDevice CurrentDevice
        {
            get
            {
                var deviceEnumerator = new MMDeviceEnumerator();
                var device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                return device;
            }
        }
        public MMDeviceCollection Devices
        {
            get
            {
                var deviceEnumerator = new MMDeviceEnumerator();
                var devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                return devices;
            }
        }

        public void ReadyAudioEngine(int deviceNum)
        {
            player = new WaveOut();
            player.DeviceNumber = deviceNum;
            player.DesiredLatency = 200;
            player.PlaybackStopped += Player_PlaybackStopped;
            CreateEQBands();
        }

        // WASAPI does NOT work correctly with SampleAggregator!!!
        //public void SetupWasapi(MMDevice device)
        //{
        //    var device = comboDevices.SelectedItem as MMDevice;
        //    AudioClientShareMode mode = AudioClientShareMode.Exclusive;
        //    player = new WasapiOut(device, mode, false, 20);
        //    player.PlaybackStopped += Player_PlaybackStopped;

        //    //device.AudioEndpointVolume.MasterVolumeLevelScalar = (float)volumeSlider.Value;
        //}

        private void Player_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                UnloadPlayer();
            }
        }

        public void LoadSong(string FileName, double InitialVolume = 0.0)
        {
            reader?.Dispose();
            reader = new AudioFileReader(FileName);
            reader.Volume = (float)InitialVolume;

            equalizer = new Equalizer(reader, _bands);

            aggregator = new SampleAggregator(equalizer, REQUIRED_SAMPLE_SIZE);
            //Debug.WriteLine($"SAMPLE RATE = {reader.WaveFormat.SampleRate}");
            aggregator.NotificationCount = reader.WaveFormat.SampleRate / 100;
            aggregator.PerformFFT = true;
            aggregator.FftCalculated += (s, a) => OnFftCalculated(a);
            aggregator.MaximumCalculated += (s, a) => OnMaximumCalculated(a);

            sampleProvider = new MeteringSampleProvider(aggregator);
            sampleProvider.StreamVolume += SampleProvider_StreamVolume;

            player.Init(sampleProvider);
            
            CreateWaveform();
            StopPlayback();
            SongLoaded = true;
        }

        
        public double[] SoundLevel
        {
            get => _volume;
            set
            {
                _volume = value;
                OnPropertyChanged("SoundLevel");
            }
        }

        private void SampleProvider_StreamVolume(object sender, StreamVolumeEventArgs e)
        {
            // running on worker thread
            SoundLevel = new double[] { e.MaxSampleValues[0] * 100, e.MaxSampleValues[1] * 100 };
        }


        public double[] FFTUpdate
        {
            get => _fftResult;
            set
            {
                _fftResult = value;
                OnPropertyChanged("FFTUpdate");
            }
        }

        private void OnFftCalculated(FftEventArgs e)
        {
            var complexNumbers = e.Result;
            double[] fftResult = new double[complexNumbers.Length];

            for (int i = 0; i < complexNumbers.Length / 2; i++)
            {
                fftResult[i] = Math.Sqrt(complexNumbers[i].X * complexNumbers[i].X + complexNumbers[i].Y * complexNumbers[i].Y);
            }

            FFTUpdate = fftResult;
        }
        

        public double MaxFrequency
        {
            get => _maxFreq;
            set
            {
                _maxFreq = value;
                OnPropertyChanged("MaxFrequency");
            }
        }

        private void OnMaximumCalculated(MaxSampleEventArgs e)
        {
            //double dbValue = 20 * Math.Log10((double)e.MaxSample);
            //MaxFrequency = e.MaxSample;
        }


        public int FrequencyBinIndex(double frequency)
        {
            var bin = (int)Math.Floor(frequency * aggregator.FFTLength / reader.WaveFormat.SampleRate);
            return bin;
        }




        public float Band1
        {
            get { return _bands[2].Gain; }
            set
            {
                if (_bands[2].Gain != value)
                {
                    _bands[2].Gain = value;
                    equalizer?.Update();
                }
            }
        }

        public float Band2
        {
            get { return _bands[4].Gain; }
            set
            {
                if (_bands[4].Gain != value)
                {
                    _bands[4].Gain = value;
                    equalizer?.Update();
                }
            }
        }

        public float Band3
        {
            get { return _bands[6].Gain; }
            set
            {
                if (_bands[6].Gain != value)
                {
                    _bands[6].Gain = value;
                    equalizer?.Update();
                }
            }
        }

        public float Band4
        {
            get { return _bands[9].Gain; }
            set
            {
                if (_bands[9].Gain != value)
                {
                    _bands[9].Gain = value;
                    equalizer?.Update();
                }
            }
        }

        public void BandUpdate(double[] semiParameters)
        {
            // first flatten all frequencies
            foreach (var a in _bands)
            {
                a.Gain = 0;
                a.Bandwidth = 0.8f;
            }

            // then boost or cut the closest (lower) value
            EqualizerBand band = null;
            if (semiParameters[0] > 0)
            {
                foreach (var b in _bands)
                {
                    if (b.Frequency <= semiParameters[0])
                    {
                        band = b;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (band != null)
            {
                band.Gain = (float)semiParameters[2];
                band.Bandwidth = (float)semiParameters[1];
            }
            equalizer?.Update();
        }

        private void CreateEQBands()
        {
            _bands = new EqualizerBand[]
            {
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 100, Gain = 0},
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 200, Gain = 0},
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 250, Gain = 0}, // [2] LO
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 400, Gain = 0},
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 500, Gain = 0}, // [4] LO-MID
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 800, Gain = 0},
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 1000, Gain = 0}, // [6] HI-MID
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 1200, Gain = 0},
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 2400, Gain = 0},
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 3500, Gain = 0}, // [9] HI
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 4800, Gain = 0},
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 9600, Gain = 0},
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 15000, Gain = 0}
            };
        }

        private void CreateWaveform()
        {
            xPos = 2;
            topPoints = new List<Point>();
            bottomPoints = new List<Point>();

            var temp = reader.Volume;
            reader.Volume = 1;

            var samplesPerSecond = (reader.WaveFormat.SampleRate * reader.WaveFormat.Channels) / 4;
            var readBuffer = new float[samplesPerSecond];

            int samplesRead;
            do
            {
                samplesRead = reader.Read(readBuffer, 0, samplesPerSecond);
                if (samplesRead > 0)
                {
                    var max = readBuffer.Take(samplesRead).Max();
                    topPoints.Add(new Point(xPos, yScale + max * yScale));
                    bottomPoints.Add(new Point(xPos, yScale - max * yScale));
                    xPos += 2;
                }
            } while (samplesRead > 0);

            reader.Volume = temp;
        }


        public double Duration
        {
            get => reader != null ? reader.TotalTime.TotalSeconds : 0;
        }

        public double Minutes
        {
            get => reader != null ? reader.TotalTime.Minutes : 0;
        }

        public double Seconds
        {
            get => reader != null ? reader.TotalTime.Seconds : 0;
        }

        public TimeSpan CurrentTime
        {
            get => reader != null ? reader.CurrentTime : new TimeSpan(0);
            set
            {
                if (reader != null)
                {
                    reader.CurrentTime = value;
                }
            }
        }

        public double CurrentMinutes
        {
            get => reader != null ? reader.CurrentTime.Minutes : 0;
        }

        public double CurrentSeconds
        {
            get => reader != null ? reader.CurrentTime.Seconds : 0;
        }


        public void ChangeVolume(float Value)
        {
            if (reader != null)
            {
                reader.Volume = Value;
            }
        }



        /** Play the current song if there is one loaded */
        public void StartPlayback()
        {
            if (player != null)
            {
                //EnableControls(true);
                player.Play();
            }
        }

        /** Pause the current song if there is one loaded */
        public void PausePlayback()
        {
            if (player != null)
            {
                player.Pause();
                //EnableControls(false);
            }
        }

        /** Stop and rewind the current song if there is one loaded */
        public void StopPlayback()
        {
            if (player != null)
            {
                player.Stop();
                if (reader != null)
                {
                    reader.Position = 0;
                }
                //EnableControls(false);
            }
        }

        /** Toggle playback of the current song if there is one loaded */
        public void TogglePlayback()
        {
            if (player != null)
            {
                if (player.PlaybackState == PlaybackState.Playing)
                {
                    PausePlayback();
                }
                else
                {
                    StartPlayback();
                }
            }
        }

        public PlaybackState PlayerState
        {
            get
            {
                if (player != null)
                {
                    return player.PlaybackState;
                } else
                {
                    return PlaybackState.Stopped;
                }
            }
        }

        public void UnloadPlayer()
        {
            if (reader != null)
            {
                reader.Dispose();
                reader = null;
            }
            if (player != null)
            {
                player.Stop();
                player.Dispose();
                player = null;
            }
        }
    }

    [Serializable]
    public class Waveform
    {
        public Waveform() { }
        public Waveform(List<Point> topPoints, List<Point> bottomPoints)
        {
            this.topPoints = topPoints;
            this.bottomPoints = bottomPoints;
        }
        public List<Point> topPoints { get; set; }
        public List<Point> bottomPoints { get; set; }
    }
}
