using Avalonia.Controls;
using ORControlPanelNew.ViewModels.Lighting;

namespace ORControlPanelNew.Views.Lighting
{
    public partial class OTLight2Dialog : Window
    {
        public OTLight2Dialog()
        {
            InitializeComponent();
            DataContext = new OTLight2ViewModel();
        }
    }
} 