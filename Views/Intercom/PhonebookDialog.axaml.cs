using Avalonia.Controls;
using Avalonia.Interactivity;
using ORControlPanelNew.ViewModels.Intercom;

namespace ORControlPanelNew.Views.Intercom
{
    public partial class PhonebookDialog : Window
    {
        public PhonebookDialog()
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

                    vm.RequestClosePhonebook += () => 
                    {
                        this.Close();
                    };
                }
            };
        }
    }
}