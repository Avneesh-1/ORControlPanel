using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ORControlPanelNew.Views
{
    public partial class AlertDialog : Window
    {
        public AlertDialog()
        {
            InitializeComponent();
        }

        public AlertDialog(string message) : this()
        {
            AlertMessageTextBlock.Text = message;
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}