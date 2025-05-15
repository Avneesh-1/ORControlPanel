using Avalonia.Controls;
using ORControlPanelNew.ViewModels.Lighting;

namespace ORControlPanelNew.Views.Lighting
{
    public partial class LaminarLightDialog : Window
    {
        public LaminarLightDialog()
        {
            InitializeComponent();
            DataContext = new LaminarLightViewModel();
        }
    }
} 