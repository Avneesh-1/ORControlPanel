using Avalonia.Controls;
using ORControlPanelNew.ViewModels.Lighting;

namespace ORControlPanelNew.Views.Lighting
{
    public partial class LightingDialog : Window
    {
        public LightingDialog()
        {
            InitializeComponent();
            DataContext = new GeneralLightingViewModel();
        }
    }
} 