using Avalonia.Controls;
using ORControlPanelNew.Views;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ORControlPanelNew.Services
{
    public class AlertService : IAlertService
    {
        private readonly Window _mainWindow;
        private readonly ConcurrentQueue<string> _alertQueue = new ConcurrentQueue<string>();
        private bool _isShowingAlert = false;

        public AlertService(Window mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public void ShowAlert(string message)
        {
            _alertQueue.Enqueue(message);
            ShowNextAlertAsync();
        }

        private async void ShowNextAlertAsync()
        {
            if (_isShowingAlert)
                return;

            _isShowingAlert = true;
            while (_alertQueue.TryDequeue(out var message))
            {
                var alertDialog = new AlertDialog(message);
                await alertDialog.ShowDialog(_mainWindow);
            }
            _isShowingAlert = false;
        }
    }
}