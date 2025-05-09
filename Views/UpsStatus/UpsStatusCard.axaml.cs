using Avalonia.Controls;
using ORControlPanelNew.ViewModels.Ups;

namespace ORControlPanelNew.Views.UpsStatus
{
    public partial class UpsStatusCard : UserControl
    {
        public UpsStatusCard()
        {
            InitializeComponent();
            DataContext = new UpsStatusViewModel();
        }
    }
}