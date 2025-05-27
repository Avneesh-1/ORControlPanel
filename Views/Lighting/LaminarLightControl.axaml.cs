using Avalonia.Controls;
using ORControlPanelNew.ViewModels.Lighting;

namespace ORControlPanelNew.Views.Lighting
{
    public partial class LaminarLightControl : UserControl
    {
        public LaminarLightControl()
        {
            InitializeComponent();
            DataContext = new LaminarLightViewModel();
        }
    }
} 