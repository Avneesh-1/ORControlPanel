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
        }

        private async void OnDeleteContactClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Contact contact)
            {
                bool confirm = await ShowDeleteConfirmation();
                if (confirm && DataContext is IntercomDialogViewModel vm)
                    vm.DeleteContactCommand.Execute(contact);
            }
        }

        private async Task<bool> ShowDeleteConfirmation()
        {
            var dialog = new Window
            {
                Title = "Confirm Delete",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new StackPanel
                {
                    Margin = new Avalonia.Thickness(16),
                    Children =
                    {
                        new TextBlock { Text = "Are you sure you want to delete this contact?", Margin = new Avalonia.Thickness(0,0,0,16) },
                        new StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            Spacing = 16,
                            Children =
                            {
                                new Button { Content = "Yes", Width = 60, Tag = true },
                                new Button { Content = "No", Width = 60, Tag = false }
                            }
                        }
                    }
                }
            };
            bool result = false;
            foreach (var child in ((StackPanel)((StackPanel)dialog.Content).Children[1]).Children)
            {
                if (child is Button btn)
                {
                    btn.Click += (_, __) =>
                    {
                        result = (bool)btn.Tag!;
                        dialog.Close();
                    };
                }
            }
            await dialog.ShowDialog(this);
            return result;
        }
    }
} 