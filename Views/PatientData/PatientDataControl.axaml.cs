using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ORControlPanelNew.Views.PatientData
{
    public partial class PatientDataControl : UserControl
    {
        public PatientDataControl()
        {
            InitializeComponent();
            this.FindControl<Button>("PatientDataButton").Click += OnPatientDataClick;
        }

        private async void OnPatientDataClick(object? sender, RoutedEventArgs e)
        {
            var dialog = new PatientDataDialog();
            await dialog.ShowDialog((Window)this.VisualRoot);
        }
    }
} 