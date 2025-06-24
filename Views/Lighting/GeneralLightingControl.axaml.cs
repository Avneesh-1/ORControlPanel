using Avalonia.Controls;
using ORControlPanelNew.ViewModels.Lighting;

namespace ORControlPanelNew.Views.Lighting
{
    public partial class GeneralLightingControl : UserControl
    {
        public GeneralLightingControl()
        {
            InitializeComponent();
            DataContext = new GeneralLightingViewModel();
        }
    }
} 