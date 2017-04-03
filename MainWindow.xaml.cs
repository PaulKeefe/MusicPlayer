using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (DataContext as ViewModel).Unload();
        }

        private void OpenFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            (DataContext as ViewModel).OpenSongFile();
            (DataContext as ViewModel).ChangePlayButtonState(false);
        }

        private void OpenFile_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void PlayToggle_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            (DataContext as ViewModel).PlayClicked();
        }

        private void PlayToggle_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            (DataContext as ViewModel).StopClicked();
            (DataContext as ViewModel).ChangePlayButtonState(false);
        }

        private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void waveformCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            (DataContext as ViewModel).WaveformWidth = e.NewSize.Width;
        }

        private void waveformCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var model = DataContext as ViewModel;
            if (model.IsLoaded)
            {
                var w = waveformCanvas.RenderedGeometry.Bounds.Width;
                var multi = model.Duration / w;
                var loc = e.GetPosition(waveformCanvas);
                var newPlace = loc.X * multi;
                model.CurrentTime = TimeSpan.FromSeconds(newPlace);
            }
        }

        private void eqLow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Amount")
            {
                (DataContext as ViewModel).EQLowAmount = (sender as Dial).Amount;
            }

        }
        private void eqMid_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Amount")
            {
                (DataContext as ViewModel).EQMidAmount = (sender as Dial).Amount;
            }
        }
        private void eqHighMid_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Amount")
            {
                (DataContext as ViewModel).EQHighMidAmount = (sender as Dial).Amount;
            }
        }
        private void eqHigh_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Amount")
            {
                (DataContext as ViewModel).EQHighAmount = (sender as Dial).Amount;
            }
        }

        
        private void PositionSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            (DataContext as ViewModel).IsLocationChanging = true;
        }

        private void PositionSlider_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var span = TimeSpan.FromSeconds(PositionSlider.Value);
            var min = span.Minutes.ToString("00");
            var sec = span.Seconds.ToString("00");
            LocationLabel.Content = $"{min}:{sec}";
        }

        private void PositionSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            var model = (DataContext as ViewModel);
            if (model.IsLoaded)
            {
                model.CurrentTime = TimeSpan.FromSeconds(PositionSlider.Value);
            }
            model.IsLocationChanging = false;
        }


        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            (DataContext as ViewModel).Engine.ChangeVolume((float)volumeSlider.Value);
        }



        private void SongTitleLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            (DataContext as ViewModel).OpenSongFile();
            (DataContext as ViewModel).ChangePlayButtonState(false);
        }

        private void comboDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (DataContext as ViewModel).Engine.ReadyAudioEngine(comboDevices.SelectedIndex);
        }

        private void TAB_PeakMeter_MouseUp(object sender, MouseButtonEventArgs e)
        {
            LED_PeakMeter.Visibility = Visibility.Visible;
            LED_SuperQ.Visibility = Visibility.Hidden;
            SpectrumView.Visibility = Visibility.Visible;
            SuperQView.Visibility = Visibility.Hidden;
        }

        private void TAB_SuperQ_MouseUp(object sender, MouseButtonEventArgs e)
        {
            LED_SuperQ.Visibility = Visibility.Visible;
            LED_PeakMeter.Visibility = Visibility.Hidden;
            SuperQView.Visibility = Visibility.Visible;
            SpectrumView.Visibility = Visibility.Hidden;
        }

        private void SuperQView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            (DataContext as ViewModel).SuperQUpdate(SuperQView.Loudness);
        }

        private void ResetEQButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            (DataContext as ViewModel).ResetEQ();
        }
    }
}
