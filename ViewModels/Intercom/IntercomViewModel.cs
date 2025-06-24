using System.Windows.Input;
using ReactiveUI;

namespace ORControlPanelNew.ViewModels.Intercom
{
    public class IntercomViewModel : ReactiveObject
    {
        public ICommand OpenDialogCommand { get; }

        public IntercomViewModel()
        {
            OpenDialogCommand = ReactiveCommand.Create(() =>
            {
                // TODO: Implement intercom dialog opening
            });
        }
    }
} 