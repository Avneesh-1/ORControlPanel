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
        }

        private void OnDeleteContactClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is object contact)
            {
                if (DataContext is IntercomDialogViewModel vm && vm.DeleteContactCommand.CanExecute(contact))
                {
                    vm.DeleteContactCommand.Execute(contact);
                }
            }
        }
    }
} 