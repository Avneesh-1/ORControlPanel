using Avalonia.Controls;

namespace ORControlPanelNew.Views.Music
{
    public partial class MusicButton : UserControl
    {
        public MusicButton()
        {
            InitializeComponent();
            this.FindControl<Button>("MusicControlButton").Click += OnMusicButtonClick;
        }

        private async void OnMusicButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var dialog = new MusicDialog();
            if (VisualRoot is Window window)
            {
                await dialog.ShowDialog(window);
            }
            else
            {
                dialog.Show();
            }
        }
    }
} 