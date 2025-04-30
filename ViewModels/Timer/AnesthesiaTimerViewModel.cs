using System;
using System.Timers;
using System.Media;
using ReactiveUI;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using ORControlPanelNew.Views.Timer;

namespace ORControlPanelNew.ViewModels.Timer
{
    public class AnesthesiaTimerViewModel : ReactiveObject
    {
        private readonly System.Timers.Timer _timer;
        private TimeSpan _remainingTime;
        private TimeSpan _initialTime;
        private bool _isRunning;
        private SoundPlayer? _alarmSound;
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
            _initialTime = TimeSpan.FromMinutes(60); // Default 60 minutes
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
                string soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "alarm.wav");
                _alarmSound = new SoundPlayer(soundPath);
                _alarmSound.LoadAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading alarm sound: {ex.Message}");
            }
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));

            // Check if 30 seconds remaining and alarm not yet triggered
            if (_remainingTime.TotalSeconds <= 30 && !_alarmTriggered)
            {
                PlayAlarm();
                _alarmTriggered = true;
            }

            // Stop at 00:00:00
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
            }
        }

        private void Reset()
        {
            Stop();
            _remainingTime = TimeSpan.Zero;  // Reset to zero instead of initial time
            _initialTime = TimeSpan.Zero;    // Also reset initial time
            _alarmTriggered = false;
            this.RaisePropertyChanged(nameof(ElapsedTime));
        }

        private async Task SetTime()
        {
            if (_parentWindow == null) return;

            Stop(); // Stop the timer while setting new time
            var dialog = new TimeInputDialog(_initialTime);
            
            // Subscribe to real-time updates
            dialog.TimeChanged += (s, newTime) => 
            {
                _remainingTime = newTime;
                this.RaisePropertyChanged(nameof(ElapsedTime));
            };
            
            var result = await dialog.ShowDialog<TimeSpan?>(_parentWindow);

            if (result.HasValue && result.Value.TotalSeconds > 0)
            {
                _initialTime = result.Value;
                _remainingTime = _initialTime;
                this.RaisePropertyChanged(nameof(ElapsedTime));
            }
        }

        private void PlayAlarm()
        {
            try
            {
                _alarmSound?.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing alarm sound: {ex.Message}");
            }
        }
    }
} 