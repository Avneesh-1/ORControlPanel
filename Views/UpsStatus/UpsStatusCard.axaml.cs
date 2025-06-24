using Avalonia.Controls;
using ORControlPanelNew.ViewModels.GasMonitoring;
using ORControlPanelNew.ViewModels.Ups;

namespace ORControlPanelNew.Views.UpsStatus
{
    public partial class UpsStatusCard : UserControl
    {
        private UpsStatusViewModel _viewModel;
        public UpsStatusCard()
        {
            InitializeComponent();
            _viewModel = DataContext as UpsStatusViewModel;
        }
    }
}