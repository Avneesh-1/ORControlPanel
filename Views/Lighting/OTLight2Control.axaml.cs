using Avalonia.Controls;
using ORControlPanelNew.ViewModels.Lighting;

namespace ORControlPanelNew.Views.Lighting
{
    public partial class OTLight2Control : UserControl
    {
        public OTLight2Control()
        {
            InitializeComponent();
            DataContext = new OTLight2ViewModel();
        }
    }
} 