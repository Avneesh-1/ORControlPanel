using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ORControlPanelNew.Models.GasMonitoring
{
    public class GasStatus : INotifyPropertyChanged
    {
        private string _name;
        private string _pressure;
        private bool _isAlert;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Pressure
        {
            get => _pressure;
            set
            {
                _pressure = value;
                OnPropertyChanged();
            }
        }

        public bool IsAlert
        {
            get => _isAlert;
            set
            {
                _isAlert = value;
                OnPropertyChanged();
            }
        }

        public GasStatus(string name)
        {
            Name = name;
            Pressure = "0.0";
            IsAlert = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}