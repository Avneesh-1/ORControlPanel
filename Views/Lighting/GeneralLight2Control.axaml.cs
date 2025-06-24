using Avalonia.Controls;
using ORControlPanelNew.ViewModels.Lighting;
using System.Reactive;
using ReactiveUI;

namespace ORControlPanelNew.Views.Lighting
{
    public partial class GeneralLight2Control : UserControl
    {
        public GeneralLight2Control()
        {
            InitializeComponent();
            DataContext = new GeneralLight2ViewModel();
        }
    }
} 