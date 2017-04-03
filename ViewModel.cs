using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MusicPlayer
{
    public class ViewModel : INotifyPropertyChanged
    {
        AudioEngine _model;

        private DispatcherTimer timer;
        
        private double[] frequencies;
        private int[] frequencyBins;


        private const double MAX_EQ_GAIN = 30;
        private const double MIN_DB_VALUE = -60;
        private const double MAX_DB_VALUE = 0;

        public ViewModel()
        {
            Prepare(new AudioEngine());
        }

        public void Prepare(AudioEngine Model)
        {
            _model = Model;
            _model.PropertyChanged += _engine_PropertyChanged;

            PrepareSpectrumAnalyzer();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;

            _isLocationChanging = false;
            PopulateDevicesCombo();
            //SetupWasapi(); method called when combo is created

            EnableControls(false);
            //PlayButton.IsEnabled = false;

            MinEQGain = -MAX_EQ_GAIN;
            MaxEQGain = MAX_EQ_GAIN;
            EQMinimumDb = MIN_DB_VALUE;
        }

        public void Unload()
        {
            _model.UnloadPlayer();
        }

        public AudioEngine Engine
        {
            get => _model;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private bool _loading = false;
        public bool IsLoading
        {
            get => _loading;
            set
            {
                _loading = value;
                OnPropertyChanged("IsLoading");
            }
        }

        private bool _songLoaded = false;
        public bool IsLoaded
        {
            get => _songLoaded;
            set
            {
                _songLoaded = value;
                OnPropertyChanged("IsLoaded");
            }
        }


        private bool _isLocationChanging;
        public bool IsLocationChanging
        {
            get => _isLocationChanging;
            set => _isLocationChanging = value;
        }

        private PointCollection _waveform = null;
        public PointCollection Waveform
        {
            get => _waveform;
            set
            {
                _waveform = value;
                OnPropertyChanged("Waveform");
            }
        }
        private double _waveformWidth = 0;
        public double WaveformWidth
        {
            get => _waveformWidth;
            set
            {
                _waveformWidth = value;
                //OnPropertyChanged("WaveformWidth");
            }
        }
        private Thickness _waveformCursorLocation = new Thickness(0);
        public Thickness WaveformCursorLocation
        {
            get => _waveformCursorLocation;
            set
            {
                _waveformCursorLocation = value;
                OnPropertyChanged("WaveformCursorLocation");
            }
        }

        private double _volume = .8;
        public double Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                _model?.ChangeVolume((float)value);
                OnPropertyChanged("Volume");
            }
        }

        private double _splLeft = 0;
        public double SoundChannelLeft
        {
            get => _splLeft;
            set
            {
                _splLeft = value;
                OnPropertyChanged("SoundChannelLeft");
            }
        }
        private double _splRight = 0;
        public double SoundChannelRight
        {
            get => _splRight;
            set
            {
                _splRight = value;
                OnPropertyChanged("SoundChannelRight");
            }
        }

        private ItemCollection _devices;
        public ItemCollection Devices
        {
            get => _devices;
            set
            {
                _devices = value;
                OnPropertyChanged("Devices");
            }
        }
        private int _deviceSelection;
        public int DeviceSelection
        {
            get => _deviceSelection;
            set
            {
                _deviceSelection = value;
                _model?.ReadyAudioEngine(value);
                OnPropertyChanged("DeviceSelection");
            }
        }

        private void PopulateDevicesCombo()
        {
            var coll = new ComboBox().Items;

            var index = 0; var selection = 0;
            foreach (var device in _model.Devices)
            {
                if (device == _model.CurrentDevice) { selection = index; }
                coll.Add(device);
                index++;
            }
            Devices = coll;
            DeviceSelection = selection;
        }

        private string _filename = "Select a Song...";
        public string FileName
        {
            get => _filename;
            set
            {
                _filename = value;
                OnPropertyChanged("FileName");
            }
        }

        private double _duration = 0;
        public double Duration
        {
            get => _duration;
            set
            {
                _duration = value;
                OnPropertyChanged("Duration");
            }
        }

        private double _songLocation = 0;
        public double SongLocation
        {
            get => _songLocation;
            set
            {
                _songLocation = value;
                var span = TimeSpan.FromSeconds(value);
                var min = span.Minutes.ToString("00");
                var sec = span.Seconds.ToString("00");
                LocationDisplay = $"{min}:{sec}";
                OnPropertyChanged("SongLocation");
            }
        }

        private double _tickFrequency = 25;
        public double TickFrequency
        {
            get => _tickFrequency;
            set
            {
                _tickFrequency = value;
                OnPropertyChanged("TickFrequency");
            }
        }

        private string _locationDisplay = "00:00";
        public string LocationDisplay
        {
            get => _locationDisplay;
            set
            {
                _locationDisplay = value;
                OnPropertyChanged("LocationDisplay");
            }
        }

        private void DisplayCurrentTime()
        {
            var min = _model.CurrentMinutes.ToString("00");
            var sec = _model.CurrentSeconds.ToString("00");
            LocationDisplay = $"{min}:{sec}";
        }

        private string _durationDisplay = "00:00";
        public string DurationDisplay
        {
            get => _durationDisplay;
            set
            {
                _durationDisplay = value;
                OnPropertyChanged("DurationDisplay");
            }
        }

        private void DisplayDuration()
        {
            var min = _model.Minutes.ToString("00");
            var sec = _model.Seconds.ToString("00");
            DurationDisplay = $"{min}:{sec}";
        }


        private double _minEQ = 0;
        public double MinEQGain
        {
            get => _minEQ;
            set
            {
                _minEQ = value;
                OnPropertyChanged("MinEQGain");
            }
        }

        private double _maxEQ = 0;
        public double MaxEQGain
        {
            get => _maxEQ;
            set
            {
                _maxEQ = value;
                OnPropertyChanged("MaxEQGain");
            }
        }

        private double _eqLow = 0;
        public double EQLow
        {
            get => _eqLow;
            set
            {
                _eqLow = value;
                OnPropertyChanged("EQLow");
            }
        }
        private double _eqMid = 0;
        public double EQMid
        {
            get => _eqMid;
            set
            {
                _eqMid = value;
                OnPropertyChanged("EQMid");
            }
        }
        private double _eqHighMid = 0;
        public double EQHighMid
        {
            get => _eqHighMid;
            set
            {
                _eqHighMid = value;
                OnPropertyChanged("EQHighMid");
            }
        }
        private double _eqHigh = 0;
        public double EQHigh
        {
            get => _eqHigh;
            set
            {
                _eqHigh = value;
                OnPropertyChanged("EQHigh");
            }
        }
        private double _eqLowAmount = 0;
        public double EQLowAmount
        {
            get => _eqLowAmount;
            set
            {
                if (!_songLoaded) return;
                _eqLowAmount = value;
                _model.Band1 = (float)value;
            }
        }
        private double _eqMidAmount = 0;
        public double EQMidAmount
        {
            get => _eqMidAmount;
            set
            {
                if (!_songLoaded) return;
                _eqMidAmount = value;
                _model.Band2 = (float)value;
            }
        }
        private double _eqHighMidAmount = 0;
        public double EQHighMidAmount
        {
            get => _eqHighMidAmount;
            set
            {
                if (!_songLoaded) return;
                _eqHighMidAmount = value;
                _model.Band3 = (float)value;
            }
        }
        private double _eqHighAmount = 0;
        public double EQHighAmount
        {
            get => _eqHighAmount;
            set
            {
                if (!_songLoaded) return;
                _eqHighAmount = value;
                _model.Band4 = (float)value;
            }
        }



        private double _eqMinimumDb;
        public double EQMinimumDb
        {
            get => _eqMinimumDb;
            set
            {
                _eqMinimumDb = value;
                OnPropertyChanged("EQMinimumDb");
            }
        }

        private double[] _eqFrequencyMagnitudes;
        public double[] EQFrequencyMagnitudes
        {
            get => _eqFrequencyMagnitudes;
            set
            {
                _eqFrequencyMagnitudes = value;
                OnPropertyChanged("EQFrequencyMagnitudes");
            }
        }


        public void SuperQUpdate(double[] loudness)
        {
            _model?.BandUpdate(loudness);
        }


        private void EnableControls(bool IsPlaying)
        {
            EQLow = 0;
            EQMid = 0;
            EQHighMid = 0;
            EQHigh = 0;
            EQLowAmount = 0;
            EQMidAmount = 0;
            EQHighMidAmount = 0;
            EQHighAmount = 0;
        }

        public void OpenSongFile()
        {
            // Stop the current song
            _model.StopPlayback();
            IsLoading = true;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Audio File (*.mp3)|*.mp3;";

            var result = dialog.ShowDialog();
            if (result == true)
            {
                _model.UnloadPlayer();

                _model.ReadyAudioEngine(_deviceSelection);
                //_model.ReadyAudioEngine(comboDevices.SelectedIndex);

                _model.LoadSong(dialog.FileName, (float)_volume);
                //_model.LoadSong(dialog.FileName, (float)volumeSlider.Value);

                FileName = dialog.SafeFileName;
                //SongTitleLabel.Content = dialog.SafeFileName;

                DisplayDuration();
                DisplayCurrentTime();

                Duration = _model.Duration;
                TickFrequency = _model.Duration / 25;
                SongLocation = 0;
                timer.Start();

                EnableControls(false);
            }
            else
            {
                IsLoading = false;
            }
        }

        private void _engine_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SongLoaded")
            {
                PrepareFrequencyBinIndexes();                
                Waveform = _model.WaveformPoints;

                _songLoaded = true;
                IsLoading = false;
            }
            else if (e.PropertyName == "SoundLevel")
            {
                var vol = _model.SoundLevel;
                //Debug.WriteLine($"SoundLevel: {vol[0]} :: {vol[1]}");
                SoundChannelLeft = vol[0];
                SoundChannelRight = vol[1];
            }
            else if (e.PropertyName == "FFTUpdate")
            {
                DisplaySpectrum(_model.FFTUpdate);
            }
            else if (e.PropertyName == "MaxFrequency")
            {
                //Debug.WriteLine($"MaxFrequency: {_model.MaxFrequency}");
            }
        }

        private void DisplaySpectrum(double[] fftArray)
        {
            double[] intensities = new double[frequencyBins.Length];

            // currently we display 19 frequencies from 25 to 20k
            for (var i = 0; i < frequencyBins.Length; i++)
            {
                // decibles for the freqency bin
                intensities[i] = 10 * Math.Log10(fftArray[frequencyBins[i]]);
            }

            EQFrequencyMagnitudes = intensities;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            PlayClicked();
        }

        public void PlayClicked()
        {
            if (_songLoaded)
            {
                _model.TogglePlayback();
                ChangePlayButtonState(_model.PlayerState == NAudio.Wave.PlaybackState.Playing);
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopClicked();
        }

        public void StopClicked()
        {
            _model.StopPlayback();
            ChangePlayButtonState(false);
        }

        private void SongTitleLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSongFile();
            ChangePlayButtonState(false);
        }


        private bool _showPlayButton = true;
        public bool ShowPlayButton
        {
            get => _showPlayButton;
            set
            {
                _showPlayButton = value;
                OnPropertyChanged("ShowPlayButton");
            }
        }
        public bool ShowPauseButton
        {
            get => !_showPlayButton;
            set
            {
                _showPlayButton = !value;
                OnPropertyChanged("ShowPauseButton");
            }
        }

        public TimeSpan CurrentTime
        {
            get => _model.CurrentTime;
            set => _model.CurrentTime = value;
        }

        public void ChangePlayButtonState(bool isPlaying)
        {
            if (isPlaying)
            {
                ShowPlayButton = false;
                ShowPauseButton = true;
            }
            else
            {
                ShowPlayButton = true;
                ShowPauseButton = false;
            }
        }



        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_model == null) { return; }
            DisplayCurrentTime();

            if (!_isLocationChanging)
            {
                var seconds = (_model.CurrentMinutes * 60) + _model.CurrentSeconds;
                //PositionSlider.Value = seconds;
                SongLocation = seconds;
                
                var left = WaveformWidth / _model.Duration * seconds;
                WaveformCursorLocation = new Thickness(left, 0, 0, 0);
            }
        }



        private void PrepareFrequencyBinIndexes()
        {
            frequencyBins = new int[frequencies.Length];
            for (var i = 0; i < frequencies.Length; i++)
            {
                frequencyBins[i] = _model.FrequencyBinIndex(frequencies[i]);
            }
        }

        private void PrepareSpectrumAnalyzer()
        {
            frequencies = new double[] { 25, 37.5, 50, 75, 100, 150, 200, 350, 500, 750, 1000, 1500, 2000, 3500, 5000, 7500, 10000, 15000, 20000 };
        }
    }
}
