using System;
using System.Timers;
using ReactiveUI;

namespace ORControlPanelNew.ViewModels.Clock
{
    public class ClockViewModel : ReactiveObject
    {
        private string _currentTime;
        private string _currentDate;
        private readonly System.Timers.Timer _timer;

        public string CurrentTime
        {
            get => _currentTime;
            set => this.RaiseAndSetIfChanged(ref _currentTime, value);
        }

        public string CurrentDate
        {
            get => _currentDate;
            set => this.RaiseAndSetIfChanged(ref _currentDate, value);
        }

        public ClockViewModel()
        {
            UpdateTime(); // Initial update
            _timer = new System.Timers.Timer(1000); // Update every second
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            UpdateTime();
        }

        private void UpdateTime()
        {
            var now = DateTime.Now;
            CurrentTime = now.ToString("HH:mm:ss");
            CurrentDate = $"{now:yyyy. MM. dd} {now:dddd}";
        }
    }
}