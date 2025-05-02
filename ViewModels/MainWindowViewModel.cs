using ReactiveUI;
using ORControlPanelNew.ViewModels.GasMonitoring;

namespace ORControlPanelNew.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        public GasMonitoringViewModel GasMonitoring { get; }

        public MainWindowViewModel()
        {
            GasMonitoring = new GasMonitoringViewModel();
        }
    }
}
