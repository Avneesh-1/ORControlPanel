using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ORControlPanelNew.Views.Intercom
{
    public partial class ConfirmDeleteDialog : Window
    {
        public bool Confirmed { get; private set; }

        public ConfirmDeleteDialog()
        {
            InitializeComponent();
        }

        public ConfirmDeleteDialog(string contactName) : this()
        {
            var textBlock = this.FindControl<TextBlock>("MessageText");
            if (textBlock != null)
                textBlock.Text = $"Are you sure you want to delete '{contactName}'?";
        }

        private void OnDeleteClick(object? sender, RoutedEventArgs e)
        {
            Confirmed = true;
            Close();
        }

        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            Confirmed = false;
            Close();
        }
    }
}
