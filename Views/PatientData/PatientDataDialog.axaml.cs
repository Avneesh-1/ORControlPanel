using Avalonia.Controls;
using ORControlPanelNew.ViewModels.PatientData;

namespace ORControlPanelNew.Views.PatientData
{
    public partial class PatientDataDialog : Window
    {
        public PatientDataDialog()
        {
            InitializeComponent();
            DataContext = new PatientDataDialogViewModel(this);
        }
    }
}