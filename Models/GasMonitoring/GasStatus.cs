using System;
using ReactiveUI;

namespace ORControlPanelNew.Models.GasMonitoring
{
    public enum GasLevel
    {
        Low,
        Normal,
        High
    }

    public class GasStatus : ReactiveObject
    {
        private string _name;
        private GasLevel _level;
        private bool _isActive;

        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public GasLevel Level
        {
            get => _level;
            set => this.RaiseAndSetIfChanged(ref _level, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => this.RaiseAndSetIfChanged(ref _isActive, value);
        }

        public double IsLow => Level == GasLevel.Low ? 1.0 : 0.2;
        public double IsNormal => Level == GasLevel.Normal ? 1.0 : 0.2;
        public double IsHigh => Level == GasLevel.High ? 1.0 : 0.2;

        public GasStatus(string name)
        {
            Name = name;
            Level = GasLevel.Normal;
            IsActive = true;
        }
    }
}
