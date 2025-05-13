using Avalonia.Controls;
using ORControlPanelNew.ViewModels.Intercom;

namespace ORControlPanelNew.Views.Intercom
{
    public partial class IntercomButton : UserControl
    {
        public IntercomButton()
        {
            InitializeComponent();
            DataContext = new IntercomViewModel();
        }
    }
} 