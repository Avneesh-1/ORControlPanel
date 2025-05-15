using Avalonia.Controls;
using ORControlPanelNew.ViewModels.Lighting;

namespace ORControlPanelNew.Views.Lighting
{
    public partial class OTLightsControl : UserControl
    {
        public OTLightsControl()
        {
            InitializeComponent();
            DataContext = new OTLightsViewModel();
        }
    }
} 