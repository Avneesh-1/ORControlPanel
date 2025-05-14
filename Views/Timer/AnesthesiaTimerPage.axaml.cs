using Avalonia.Controls;
using ORControlPanelNew.ViewModels.Timer;

namespace ORControlPanelNew.Views.Timer
{
    public partial class AnesthesiaTimerPage : UserControl
    {
        private readonly AnesthesiaTimerViewModel _viewModel;

        public AnesthesiaTimerPage()
        {
            InitializeComponent();
            _viewModel = new AnesthesiaTimerViewModel();
            DataContext = _viewModel;

            this.AttachedToVisualTree += (s, e) =>
            {
                var window = TopLevel.GetTopLevel(this) as Window;
                if (window != null)
                {
                    _viewModel.SetParentWindow(window);
                }
            };
        }
    }
} 