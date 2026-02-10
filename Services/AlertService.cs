using Avalonia.Controls;
using Avalonia.Threading;
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
            // Always dispatch to UI thread to process the queue
            Dispatcher.UIThread.InvokeAsync(ProcessAlertQueueAsync);
        }

        private async Task ProcessAlertQueueAsync()
        {
            // This method is guaranteed to run on the UI thread due to InvokeAsync above
            if (_isShowingAlert)
                return;

            _isShowingAlert = true;
            try
            {
                while (_alertQueue.TryDequeue(out var message))
                {
                    // User requested NO POPUPS. 
                    // Previously: var alertDialog = new AlertDialog(message); await alertDialog.ShowDialog(_mainWindow);
                    // Now: Just ignore/log. The UI indicators (Red Color) are handled by ViewModels properties.
                    System.Diagnostics.Debug.WriteLine($"[AlertService] Suppressed Popup: {message}");
                }
            }
            finally
            {
                _isShowingAlert = false;
                
                // Double check queue in case new items arrived while we were finishing
                if (!_alertQueue.IsEmpty)
                {
                     // Re-schedule processing
                     var _ = Dispatcher.UIThread.InvokeAsync(ProcessAlertQueueAsync);
                }
            }
        }
    }
}