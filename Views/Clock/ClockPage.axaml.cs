using Avalonia.Controls;
using ORControlPanelNew.ViewModels.Clock;

namespace ORControlPanelNew.Views.Clock
{
    public partial class ClockPage : UserControl
    {
        public ClockPage()
        {
            InitializeComponent();
            DataContext = new ClockViewModel();
        }
    }
}