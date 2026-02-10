using Avalonia.Controls;
using Avalonia.Interactivity;
using ORControlPanelNew.ViewModels.Intercom;
using System.Threading.Tasks;

namespace ORControlPanelNew.Views.Intercom
{
    public partial class IntercomDialog : Window
    {
        public IntercomDialog()
        {
            InitializeComponent();
            
            this.Opened += (s, e) => 
            {
                if (DataContext is IntercomDialogViewModel vm)
                {
                    vm.OnRequestDeleteConfirmation += async (contact) => 
                    {
                        var confirmDialog = new ConfirmDeleteDialog(contact.Name);
                        await confirmDialog.ShowDialog(this);
                        return confirmDialog.Confirmed;
                    };
                }
            };
        }
    }
}