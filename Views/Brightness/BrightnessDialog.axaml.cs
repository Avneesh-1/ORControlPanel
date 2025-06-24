using Avalonia.Controls;
using ORControlPanelNew.ViewModels.Brightness;

namespace ORControlPanelNew.Views.Brightness
{
    public partial class BrightnessDialog : Window
    {
        public BrightnessDialog()
        {
            InitializeComponent();
            DataContext = new BrightnessViewModel();

            // Subscribe to the Deactivated event to close the window when it loses focus
            this.Deactivated += (sender, args) =>
            {
                this.Close();
            };
        }
    }
}