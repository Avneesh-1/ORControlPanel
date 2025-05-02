using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ORControlPanelNew.Views.GasMonitoring
{
    public partial class GasMonitoringView : UserControl
    {
        public GasMonitoringView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
} 