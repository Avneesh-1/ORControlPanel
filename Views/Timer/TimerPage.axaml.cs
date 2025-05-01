using Avalonia.Controls;
using ORControlPanelNew.ViewModels.Timer;

namespace ORControlPanelNew.Views.Timer
{
    public partial class TimerPage : UserControl
    {
        public TimerPage()
        {
            InitializeComponent();
            DataContext = new TimerPageViewModel();
        }
    }
}
