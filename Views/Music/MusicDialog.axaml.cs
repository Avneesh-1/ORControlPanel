using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Controls.Primitives;
using System;
using System.IO;
using System.Reflection;
using Avalonia.Media;
using System.Media;
using System.Runtime.InteropServices;

namespace ORControlPanelNew.Views.Music
{
    public partial class MusicDialog : Window
    {
        private SoundPlayer? _soundPlayer;
        private bool _isPlaying = false;
        private string _currentTrack = "";
        private bool _isTrackSelected = false;

        // Windows API for volume control
        [DllImport("winmm.dll")]
        private static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

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
                // Set initial selection to empty
                trackComboBox.SelectedIndex = -1;
            }
        }

        private void InitializeSoundPlayer()
        {
            try
            {
                _soundPlayer = new SoundPlayer();
                
                // Try multiple possible paths for the sound file
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
                        _soundPlayer.SoundLocation = path;
                        _soundPlayer.Load();
                        break;
                    }
                }

                // Set initial volume to 100%
                if (OperatingSystem.IsWindows())
                {
                    SetVolume(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading sound file: {ex.Message}");
            }
        }

        private void SetVolume(int volume)
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    // Convert volume (0-100) to the format expected by Windows
                    uint volumeValue = (uint)((volume * 65535) / 100);
                    uint volumeAll = volumeValue | (volumeValue << 16);
                    waveOutSetVolume(IntPtr.Zero, volumeAll);
                }
                else
                {
                    // For non-Windows platforms, we'll use the system's default volume
                    Console.WriteLine($"Volume control is currently only supported on Windows. Current volume: {volume}%");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting volume: {ex.Message}");
            }
        }

        private void SetupEventHandlers()
        {
            // Play/Pause button
            var playPauseButton = this.FindControl<Button>("PlayPauseButton");
            if (playPauseButton != null)
            {
                playPauseButton.Click += OnPlayPauseClick;
                // Initially disable the play button until a track is selected
                playPauseButton.IsEnabled = false;
            }

            // Volume slider
            var volumeSlider = this.FindControl<Slider>("VolumeSlider");
            if (volumeSlider != null)
            {
                volumeSlider.PropertyChanged += (s, e) =>
                {
                    if (e.Property.Name == "Value")
                    {
                        SetVolume((int)volumeSlider.Value);
                    }
                };
            }

            // Track selection
            var trackComboBox = this.FindControl<ComboBox>("TrackComboBox");
            if (trackComboBox != null)
            {
                trackComboBox.SelectionChanged += OnTrackSelectionChanged;
            }
        }

        private void OnPlayPauseClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_soundPlayer == null || !_isTrackSelected) return;

            try
            {
                if (_isPlaying)
                {
                    _soundPlayer.Stop();
                    _isPlaying = false;
                }
                else
                {
                    _soundPlayer.Play();
                    _isPlaying = true;
                }

                // Update button text
                var playPauseButton = this.FindControl<Button>("PlayPauseButton");
                if (playPauseButton != null)
                {
                    playPauseButton.Content = _isPlaying ? "⏸️ Pause" : "▶️ Play";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing sound: {ex.Message}");
            }
        }

        private void OnTrackSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_soundPlayer == null) return;

            try
            {
                var comboBox = sender as ComboBox;
                if (comboBox?.SelectedItem is ComboBoxItem selectedItem)
                {
                    _currentTrack = selectedItem.Content?.ToString() ?? "";
                    _isTrackSelected = !string.IsNullOrEmpty(_currentTrack);
                    
                    // Stop current playback
                    _soundPlayer.Stop();
                    _isPlaying = false;

                    // Update button text and state
                    var playPauseButton = this.FindControl<Button>("PlayPauseButton");
                    if (playPauseButton != null)
                    {
                        playPauseButton.Content = "▶️ Play";
                        playPauseButton.IsEnabled = _isTrackSelected;
                    }

                    // For now, we only have one track, but this can be expanded
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
                                _soundPlayer.SoundLocation = path;
                                _soundPlayer.Load();
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error changing track: {ex.Message}");
            }
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            base.OnClosing(e);
            _soundPlayer?.Dispose();
        }
    }
} 