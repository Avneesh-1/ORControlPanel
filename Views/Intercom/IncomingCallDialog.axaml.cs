using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ORControlPanelNew.Views.Intercom
{
    public partial class IncomingCallDialog : Window
    {
        public IncomingCallDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
