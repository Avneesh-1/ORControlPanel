using Avalonia.Controls;
using ORControlPanelNew.ViewModels.Intercom;

namespace ORControlPanelNew.Views.Intercom
{
    public partial class IntercomButton : UserControl
    {
        public IntercomButton()
        {
            InitializeComponent();
            IntercomControlButton.Click += (s, e) =>
            {
                var dialog = new IntercomDialog();
                if (this.VisualRoot is Window parent)
                    dialog.ShowDialog(parent);
                else
                    dialog.Show();
            };
            DataContext = new IntercomViewModel();
        }
    }
} 