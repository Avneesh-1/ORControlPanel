using ReactiveUI;
using ORControlPanelNew.ViewModels.GasMonitoring;
using ORControlPanelNew.ViewModels.Temperature;
using ORControlPanelNew.ViewModels.Ups;
using System.Reactive;

namespace ORControlPanelNew.ViewModels
{
    public enum FooterTab
    {
        GeneralLights,
        OTLights,
        LaminarLight,
        PatientData,
        Music
    }

    public class MainWindowViewModel : ReactiveObject
    {
        public GasMonitoringViewModel GasMonitoring { get; }
        public TemperatureViewModel TemperatureMonitoring { get; }
        public UpsStatusViewModel UpsStatus { get; }
        
        private FooterTab _selectedTab = FooterTab.GeneralLights;
        public FooterTab SelectedTab
        {
            get => _selectedTab;
            set => this.RaiseAndSetIfChanged(ref _selectedTab, value);
        }

        public ReactiveCommand<FooterTab, Unit> SetTabCommand { get; }

        public MainWindowViewModel()
        {
            GasMonitoring = new GasMonitoringViewModel();
            TemperatureMonitoring = new TemperatureViewModel();
            UpsStatus = new UpsStatusViewModel();
            SetTabCommand = ReactiveCommand.Create<FooterTab>(tab => SelectedTab = tab);
        }
    }
}
