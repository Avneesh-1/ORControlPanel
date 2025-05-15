using Avalonia.Controls;
using ORControlPanelNew.ViewModels.Lighting;

namespace ORControlPanelNew.Views.Lighting
{
    public partial class GeneralLight2Dialog : Window
    {
        public GeneralLight2Dialog()
        {
            InitializeComponent();
            DataContext = new GeneralLight2ViewModel();
        }
    }
} 