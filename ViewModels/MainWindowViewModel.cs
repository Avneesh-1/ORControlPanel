using ReactiveUI;
using ORControlPanelNew.ViewModels.GasMonitoring;
using ORControlPanelNew.ViewModels.Temperature;
using ORControlPanelNew.ViewModels.Ups;

namespace ORControlPanelNew.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        public GasMonitoringViewModel GasMonitoring { get; }
        public TemperatureViewModel TemperatureMonitoring { get; }
        public UpsStatusViewModel UpsStatus { get; }
        

        public MainWindowViewModel()
        {
            GasMonitoring = new GasMonitoringViewModel();
            TemperatureMonitoring = new TemperatureViewModel();
            UpsStatus = new UpsStatusViewModel();
        }
    }
}
