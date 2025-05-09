using Avalonia.Controls;
using ORControlPanelNew.ViewModels.Temperature;

namespace ORControlPanelNew.Views.Temperature
{
    public partial class TemperatureCard : UserControl
    {
        public TemperatureCard()
        {
            InitializeComponent();
            DataContext = new TemperatureViewModel();
        }
    }
} 