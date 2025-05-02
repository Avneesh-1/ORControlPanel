using System.Collections.ObjectModel;
using ReactiveUI;
using ORControlPanelNew.Models.GasMonitoring;
using System.Linq;
using System;

namespace ORControlPanelNew.ViewModels.GasMonitoring
{
    public class GasMonitoringViewModel : ReactiveObject
    {
        private ObservableCollection<GasStatus> _gases = new();
        private readonly Random _random = new();

        public ObservableCollection<GasStatus> Gases
        {
            get => _gases;
            set => this.RaiseAndSetIfChanged(ref _gases, value);
        }

        public GasMonitoringViewModel()
        {
            InitializeGases();
            // Simulate some initial states
            SimulateGasStates();
        }

        private void InitializeGases()
        {
            Gases = new ObservableCollection<GasStatus>
            {
                new GasStatus("O₂"),     // Oxygen
                new GasStatus("N₂O"),    // Nitrogen
                new GasStatus("CO₂"),    // Carbon Dioxide
                new GasStatus("AIR 4"),  // Air 4
                new GasStatus("AIR 7"),  // Air 7
                new GasStatus("VAC")     // Vacuum
            };
        }

        private void SimulateGasStates()
        {
            // Set some initial states for testing
            UpdateGasStatus("O₂", GasLevel.Normal);
            UpdateGasStatus("N₂O", GasLevel.Low);
            UpdateGasStatus("CO₂", GasLevel.High);
            UpdateGasStatus("AIR 4", GasLevel.Normal);
            UpdateGasStatus("AIR 7", GasLevel.Normal);
            UpdateGasStatus("VAC", GasLevel.Low);
        }

        public void UpdateGasStatus(string gasName, GasLevel level)
        {
            var gas = Gases.FirstOrDefault(g => g.Name == gasName);
            if (gas != null)
            {
                gas.Level = level;
                gas.IsActive = true;
                // Notify UI of the change
                this.RaisePropertyChanged(nameof(Gases));
            }
        }

        public void ToggleGasActive(string gasName)
        {
            var gas = Gases.FirstOrDefault(g => g.Name == gasName);
            if (gas != null)
            {
                gas.IsActive = !gas.IsActive;
                // Notify UI of the change
                this.RaisePropertyChanged(nameof(Gases));
            }
        }
    }
} 