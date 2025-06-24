using System;
using System.Reactive;
using System.Timers;
using ReactiveUI;

namespace ORControlPanelNew.ViewModels.Timer
{
    public class TimerPageViewModel : ReactiveObject
    {
        private System.Timers.Timer _timer;  // The actual timer object
        private TimeSpan _elapsed;           // Stores the elapsed time
        private string _elapsedTime = "00:00:00";  // Formatted time string

        // Property that updates the UI when changed
        public string ElapsedTime
        {
            get => _elapsedTime;
            set => this.RaiseAndSetIfChanged(ref _elapsedTime, value);
        }

        // Commands for the buttons
        public ReactiveCommand<Unit, Unit> StartCommand { get; }
        public ReactiveCommand<Unit, Unit> StopCommand { get; }
        public ReactiveCommand<Unit, Unit> ResetCommand { get; }

        public TimerPageViewModel()
        {
            // Initialize timer to tick every 1000ms (1 second)
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += OnTimedEvent;  // Subscribe to timer events

            // Create commands for the buttons
            StartCommand = ReactiveCommand.Create(StartTimer);
            StopCommand = ReactiveCommand.Create(StopTimer);
            ResetCommand = ReactiveCommand.Create(ResetTimer);
        }

        private void StartTimer()
        {
            _timer.Start();  // Start the timer
        }

        private void StopTimer()
        {
            _timer.Stop();   // Stop the timer
        }

        private void ResetTimer()
        {
            _timer.Stop();
            _elapsed = TimeSpan.Zero;  // Reset elapsed time to zero
            UpdateElapsedTime();       // Update the display
        }

        private void OnTimedEvent(object? sender, ElapsedEventArgs e)
        {
            _elapsed = _elapsed.Add(TimeSpan.FromSeconds(1));  // Add one second
            UpdateElapsedTime();                               // Update the display
        }

        private void UpdateElapsedTime()
        {
            // Format: HH:MM:SS with leading zeros
            ElapsedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", 
                _elapsed.Hours, 
                _elapsed.Minutes, 
                _elapsed.Seconds);
        }
    }
}