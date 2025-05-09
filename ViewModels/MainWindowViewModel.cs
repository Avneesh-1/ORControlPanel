using ReactiveUI;
using ORControlPanelNew.ViewModels.GasMonitoring;
using ORControlPanelNew.ViewModels.Temperature;
using ORControlPanelNew.ViewModels.Ups;
using ORControlPanelNew.Services;

namespace ORControlPanelNew.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        public GasMonitoringViewModel GasMonitoring { get; }
        public TemperatureViewModel TemperatureMonitoring { get; }
        public UpsStatusViewModel UpsStatus { get; }

        public MainWindowViewModel(IAlertService alertService)
        {
            GasMonitoring = new GasMonitoringViewModel(alertService);
            TemperatureMonitoring = new TemperatureViewModel();
            UpsStatus = new UpsStatusViewModel(alertService);
        }
    }
}