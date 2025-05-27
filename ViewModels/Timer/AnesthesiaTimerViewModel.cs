using System;
using System.Timers;
using ReactiveUI;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using ORControlPanelNew.Views.Timer;
//using NAudio.Wave;

namespace ORControlPanelNew.ViewModels.Timer
{
    public class AnesthesiaTimerViewModel : ReactiveObject
    {
        private readonly System.Timers.Timer _timer;
        private TimeSpan _remainingTime;
        private TimeSpan _initialTime;
        private bool _isRunning;
        //private WaveOutEvent? _waveOut;
        //private WaveFileReader? _waveReader;
        private bool _alarmTriggered;
        private Window? _parentWindow;

        public string ElapsedTime => _remainingTime.ToString(@"hh\:mm\:ss");

        public bool IsRunning
        {
            get => _isRunning;
            set => this.RaiseAndSetIfChanged(ref _isRunning, value);
        }

        public ReactiveCommand<Unit, Unit> StartCommand { get; }
        public ReactiveCommand<Unit, Unit> StopCommand { get; }
        public ReactiveCommand<Unit, Unit> ResetCommand { get; }
        public ReactiveCommand<Unit, Unit> SetTimeCommand { get; }

        public AnesthesiaTimerViewModel()
        {
            _initialTime = TimeSpan.Zero;
            _remainingTime = _initialTime;
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += Timer_Elapsed;

            StartCommand = ReactiveCommand.Create(Start);
            StopCommand = ReactiveCommand.Create(Stop);
            ResetCommand = ReactiveCommand.Create(Reset);
            SetTimeCommand = ReactiveCommand.CreateFromTask(async () => await SetTime());

            InitializeAlarmSound();
        }

        public void SetParentWindow(Window window)
        {
            _parentWindow = window;
        }

        private void InitializeAlarmSound()
        {
            try
            {
                string soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "digital-alarm-buzzer-992.wav");
                if (!File.Exists(soundPath))
                {
                    Console.WriteLine($"Warning: Alarm sound file not found at {soundPath}");
                    return;
                }
                //_waveReader = new WaveFileReader(soundPath);
                //_waveOut = new WaveOutEvent();
                //_waveOut.Init(_waveReader);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing alarm sound: {ex.Message}");
            }
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));

            if (_remainingTime.TotalSeconds <= 30 && !_alarmTriggered)
            {
                PlayAlarm();
                _alarmTriggered = true;
            }

            if (_remainingTime.TotalSeconds <= 0)
            {
                _remainingTime = TimeSpan.Zero;
                Stop();
            }

            this.RaisePropertyChanged(nameof(ElapsedTime));
        }

        private void Start()
        {
            if (!IsRunning)
            {
                _timer.Start();
                IsRunning = true;
            }
        }

        private void Stop()
        {
            if (IsRunning)
            {
                _timer.Stop();
                IsRunning = false;
                StopAlarm();
            }
        }

        private void Reset()
        {
            Stop();
            _remainingTime = TimeSpan.Zero;
            _initialTime = TimeSpan.Zero;
            _alarmTriggered = false;
            this.RaisePropertyChanged(nameof(ElapsedTime));
        }

        private async Task SetTime()
        {
            if (_parentWindow == null) return;

            Stop();
            var dialog = new TimeInputDialog(_initialTime);
            dialog.TimeChanged += (s, newTime) => 
            {
                _remainingTime = newTime;
                this.RaisePropertyChanged(nameof(ElapsedTime));
            };
            var result = await dialog.ShowDialog<TimeSpan?>(_parentWindow);

            if (result.HasValue)
            {
                if (result.Value == TimeSpan.MaxValue)
                {
                    // Start button was clicked in the dialog
                    _initialTime = dialog.SelectedTime;
                    _remainingTime = _initialTime;
                    this.RaisePropertyChanged(nameof(ElapsedTime));
                    Start();
                }
                else if (result.Value.TotalSeconds > 0)
                {
                    _initialTime = result.Value;
                    _remainingTime = _initialTime;
                    this.RaisePropertyChanged(nameof(ElapsedTime));
                }
            }
        }

        private void PlayAlarm()
        {
            try
            {
                //if (_waveOut != null && _waveReader != null)
                //{
                //    _waveReader.Position = 0;
                //    _waveOut.Play();
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing alarm sound: {ex.Message}");
            }
        }

        private void StopAlarm()
        {
            try
            {
                //_waveOut?.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping alarm sound: {ex.Message}");
            }
        }
    }
} 