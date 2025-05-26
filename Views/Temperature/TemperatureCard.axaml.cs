using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using ORControlPanelNew.ViewModels.Temperature;

namespace ORControlPanelNew.Views.Temperature
{
    public partial class TemperatureCard : UserControl
    {
        private TemperatureViewModel _viewModel;

        public TemperatureCard()
        {
            InitializeComponent();
            // Access the DataContext instead of creating a new instance
            _viewModel = DataContext as TemperatureViewModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }


    }
}