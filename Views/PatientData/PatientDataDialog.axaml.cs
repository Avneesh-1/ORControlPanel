using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ORControlPanelNew.Views.PatientData
{
    public partial class PatientDataDialog : Window
    {
        public PatientDataDialog()
        {
            InitializeComponent();
        }

        private void OnCloseClick(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
} 