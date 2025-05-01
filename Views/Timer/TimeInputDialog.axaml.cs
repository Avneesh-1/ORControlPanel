using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace ORControlPanelNew.Views.Timer
{
    public partial class TimeInputDialog : Window, INotifyPropertyChanged
    {
        private int _hours;
        private int _minutes;
        private int _seconds;

        public new event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<TimeSpan>? TimeChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (propertyName == nameof(Hours) || propertyName == nameof(Minutes) || propertyName == nameof(Seconds))
            {
                TimeChanged?.Invoke(this, SelectedTime);
            }
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public int Hours
        {
            get => _hours;
            set => SetField(ref _hours, Math.Max(0, value));
        }

        public int Minutes
        {
            get => _minutes;
            set => SetField(ref _minutes, Math.Max(0, Math.Min(59, value)));
        }

        public int Seconds
        {
            get => _seconds;
            set => SetField(ref _seconds, Math.Max(0, Math.Min(59, value)));
        }

        public TimeSpan SelectedTime => TimeSpan.FromSeconds(Hours * 3600 + Minutes * 60 + Seconds);

        public TimeInputDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        public TimeInputDialog(TimeSpan currentTime) : this()
        {
            Hours = currentTime.Hours;
            Minutes = currentTime.Minutes;
            Seconds = currentTime.Seconds;
        }

        private void OnOKClick(object sender, RoutedEventArgs e)
        {
            Close(SelectedTime);
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close(null);
        }
    }
} 