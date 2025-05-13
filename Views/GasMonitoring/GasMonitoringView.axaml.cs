using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ORControlPanelNew.ViewModels.GasMonitoring;

namespace ORControlPanelNew.Views.GasMonitoring
{
    public partial class GasMonitoringView : UserControl
    {
        private GasMonitoringViewModel _viewModel;

        public GasMonitoringView()
        {
            InitializeComponent();
            _viewModel = new GasMonitoringViewModel();
            DataContext = _viewModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

     
    }
}