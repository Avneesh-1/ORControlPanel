using Avalonia.Controls;
using ORControlPanelNew.ViewModels.Lighting;

namespace ORControlPanelNew.Views.Lighting
{
    public partial class OTLightsDialog : Window
    {
        public OTLightsDialog()
        {
            InitializeComponent();
            DataContext = new OTLightsViewModel();
        }
    }
} 