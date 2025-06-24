using Avalonia.Controls;
using ORControlPanelNew.ViewModels.Brightness;

namespace ORControlPanelNew.Views.Brightness
{
    public partial class BrightnessControl : UserControl
    {
        public BrightnessControl()
        {
            InitializeComponent();
            DataContext = new BrightnessViewModel();
        }
    }
} 