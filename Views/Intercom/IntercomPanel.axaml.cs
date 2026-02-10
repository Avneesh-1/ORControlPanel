using Avalonia.Controls;
using Avalonia.Interactivity;
using ORControlPanelNew.ViewModels.Intercom;

namespace ORControlPanelNew.Views.Intercom
{
    public partial class IntercomPanel : UserControl
    {
        public IntercomPanel()
        {
            InitializeComponent();
            OpenPhonebookButton.Click += (s, e) =>
            {
                var dialog = new PhonebookDialog();
                dialog.DataContext = this.DataContext;
                if (this.VisualRoot is Window parent)
                    dialog.ShowDialog(parent);
                else
                    dialog.Show();
            };
        }
    }
}