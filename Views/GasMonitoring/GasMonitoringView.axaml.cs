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
            // Access the DataContext instead of creating a new instance
            _viewModel = DataContext as GasMonitoringViewModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

     
    }
}