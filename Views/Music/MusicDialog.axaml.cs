using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Controls.Primitives;
using System;
using System.IO;
using System.Reflection;
using Avalonia.Media;
//using NAudio.Wave;

namespace ORControlPanelNew.Views.Music
{
    public partial class MusicDialog : Window
    {
        //private WaveOutEvent? _waveOut;
        //private WaveFileReader? _waveReader;
        private bool _isPlaying = false;
        private string _currentTrack = "";
        private bool _isTrackSelected = false;

        public MusicDialog()
        {
            InitializeComponent();
            InitializeSoundPlayer();
            SetupEventHandlers();
            SetInitialTrackSelection();
        }

        private void SetInitialTrackSelection()
        {
            var trackComboBox = this.FindControl<ComboBox>("TrackComboBox");
            if (trackComboBox != null)
            {
                trackComboBox.SelectedIndex = -1;
            }
        }

        private void InitializeSoundPlayer()
        {
            try
            {
                string[] possiblePaths = new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "digital-alarm-buzzer-992.wav"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Sounds", "digital-alarm-buzzer-992.wav"),
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "Assets", "Sounds", "digital-alarm-buzzer-992.wav")
                };

                //foreach (string path in possiblePaths)
                //{
                //    if (File.Exists(path))
                //    {
                //        _waveReader = new WaveFileReader(path);
                //        _waveOut = new WaveOutEvent();
                //        _waveOut.Init(_waveReader);
                //        break;
                //    }
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading sound file: {ex.Message}");
            }
        }

        private void SetupEventHandlers()
        {
            var playPauseButton = this.FindControl<Button>("PlayPauseButton");
            if (playPauseButton != null)
            {
                playPauseButton.Click += OnPlayPauseClick;
                playPauseButton.IsEnabled = false;
            }

            var volumeSlider = this.FindControl<Slider>("VolumeSlider");
            //if (volumeSlider != null)
            //{
            //    volumeSlider.PropertyChanged += (s, e) =>
            //    {
            //        if (e.Property.Name == "Value" && _waveOut != null)
            //        {
            //            _waveOut.Volume = (float)volumeSlider.Value / 100f;
            //        }
            //    };
            //}

            var trackComboBox = this.FindControl<ComboBox>("TrackComboBox");
            if (trackComboBox != null)
            {
                trackComboBox.SelectionChanged += OnTrackSelectionChanged;
            }
        }

        private void OnPlayPauseClick(object? sender, RoutedEventArgs e)
        {
            //if (_waveOut == null || _waveReader == null) return;

            if (_isPlaying)
            {
                //_waveOut.Stop();
                _isPlaying = false;
                if (sender is Button button)
                {
                    button.Content = "▶️ Play";
                }
            }
            else
            {
                //_waveReader.Position = 0;
                //_waveOut.Play();
                _isPlaying = true;
                if (sender is Button button)
                {
                    button.Content = "⏸️ Pause";
                }
            }
        }

        private void OnTrackSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            //if (_waveOut == null || _waveReader == null) return;

            var comboBox = sender as ComboBox;
            if (comboBox?.SelectedItem is ComboBoxItem selectedItem)
            {
                _currentTrack = selectedItem.Content?.ToString() ?? "";
                _isTrackSelected = !string.IsNullOrEmpty(_currentTrack);
                
                //_waveOut.Stop();
                _isPlaying = false;

                var playPauseButton = this.FindControl<Button>("PlayPauseButton");
                if (playPauseButton != null)
                {
                    playPauseButton.Content = "▶️ Play";
                    playPauseButton.IsEnabled = _isTrackSelected;
                }

                if (_currentTrack == "Track 1")
                {
                    string[] possiblePaths = new[]
                    {
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "digital-alarm-buzzer-992.wav"),
                        Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Sounds", "digital-alarm-buzzer-992.wav"),
                        Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "Assets", "Sounds", "digital-alarm-buzzer-992.wav")
                    };

                    foreach (string path in possiblePaths)
                    {
                        if (File.Exists(path))
                        {
                            //_waveReader = new WaveFileReader(path);
                            //_waveOut.Init(_waveReader);
                            break;
                        }
                    }
                }
            }
        }
    }
} 