using System.Collections.ObjectModel;
using System.Windows.Input;
using ReactiveUI;
using System.Linq;
using System.IO;
using System.Text.Json;
using System;
using Avalonia.Threading;
using ORControlPanelNew.Views.Intercom;
using System.Diagnostics;
using ORControlPanelNew;
using System.Threading.Tasks;

public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
    public void Execute(object? parameter) => _execute(parameter);
    public event EventHandler? CanExecuteChanged;
}

namespace ORControlPanelNew.ViewModels.Intercom
{
    public class Contact
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
    }

    public class IntercomDialogViewModel : ReactiveObject
    {
        private IncomingCallDialog? _incomingCallDialog;
        private DispatcherTimer? _ringingWatchdog;
        
        private string _dialedNumber = string.Empty;
        public string DialedNumber
        {
            get => _dialedNumber;
            set => this.RaiseAndSetIfChanged(ref _dialedNumber, value);
        }

        private string _newContactName = string.Empty;
        public string NewContactName
        {
            get => _newContactName;
            set => this.RaiseAndSetIfChanged(ref _newContactName, value);
        }

        private string _newContactNumber = string.Empty;
        public string NewContactNumber
        {
            get => _newContactNumber;
            set => this.RaiseAndSetIfChanged(ref _newContactNumber, value);
        }

        public static ObservableCollection<Contact> SharedContacts { get; } = new ObservableCollection<Contact>();
        public ObservableCollection<Contact> Contacts => SharedContacts;

        public event Func<Contact, Task<bool>>? OnRequestDeleteConfirmation;
        public event Action? RequestClosePhonebook;

        public ICommand DialCommand { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand SaveContactCommand { get; }
        public ICommand DeleteContactCommand { get; }
        public ICommand CallContactCommand { get; }
        
        public ICommand CallCommand { get; }
        public ICommand PickupCommand { get; }
        public ICommand HangupCommand { get; }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        private bool _isStatusVisible;
        public bool IsStatusVisible
        {
            get => _isStatusVisible;
            set => this.RaiseAndSetIfChanged(ref _isStatusVisible, value);
        }

        private bool _isCallActive;
        public bool IsCallActive
        {
            get => _isCallActive;
            set => this.RaiseAndSetIfChanged(ref _isCallActive, value);
        }

        public IntercomDialogViewModel()
        {
            LoadContacts();
            
            DialCommand = ReactiveCommand.Create<string>(ch => DialedNumber += ch);
            BackspaceCommand = ReactiveCommand.Create(() =>
            {
                if (!string.IsNullOrEmpty(DialedNumber))
                    DialedNumber = DialedNumber.Substring(0, DialedNumber.Length - 1);
            });
            SaveContactCommand = ReactiveCommand.Create(() =>
            {
                if (!string.IsNullOrWhiteSpace(NewContactName) && !string.IsNullOrWhiteSpace(NewContactNumber))
                {
                    DevicePort.AddContact(NewContactName, NewContactNumber);
                    LoadContacts();
                    NewContactName = string.Empty;
                    NewContactNumber = string.Empty;
                }
            });
            DeleteContactCommand = new RelayCommand(DeleteContact);
            CallContactCommand = ReactiveCommand.Create<Contact>(c => 
            {
                if (c != null)
                {
                    DialedNumber = c.Number;
                    MakeCall();
                    RequestClosePhonebook?.Invoke();
                }
            });

            // Intercom Actions
            CallCommand = ReactiveCommand.Create(MakeCall);
            PickupCommand = ReactiveCommand.Create(PickUpCall);
            HangupCommand = ReactiveCommand.Create(HangUpCall);

            // Event Subscriptions
            DevicePort.DataProcessor.OnRinging += () => Dispatcher.UIThread.InvokeAsync(ShowIncomingCallDialog);
            DevicePort.DataProcessor.OnHookDown += () => Dispatcher.UIThread.InvokeAsync(() => 
            {
                StopRingingWatchdog();
                IsCallActive = false;
                ShowStatus("Call Disconnected");
                CloseIncomingCallDialog();
            });
            DevicePort.DataProcessor.OnHookUp += () => Dispatcher.UIThread.InvokeAsync(() => 
            {
                StopRingingWatchdog();
                IsCallActive = true;
                ShowStatus("Call Connected");
                CloseIncomingCallDialog();
            });
        }

        private void StartRingingWatchdog()
        {
            StopRingingWatchdog();
            _ringingWatchdog = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _ringingWatchdog.Tick += (s, e) => 
            {
                Debug.WriteLine("[IntercomVM] Ringing Watchdog Timeout - closing dialog");
                CloseIncomingCallDialog();
                StopRingingWatchdog();
            };
            _ringingWatchdog.Start();
        }

        private void StopRingingWatchdog()
        {
            _ringingWatchdog?.Stop();
            _ringingWatchdog = null;
        }

        private void ShowStatus(string message)
        {
            StatusMessage = message;
            IsStatusVisible = true;
            Debug.WriteLine($"Intercom Status: {message}");
            
            // Auto hide after 3 seconds
             DispatcherTimer.RunOnce(() => 
             {
                 IsStatusVisible = false;
                 StatusMessage = string.Empty;
             }, TimeSpan.FromSeconds(3));
        }

        private void MakeCall()
        {
            if (string.IsNullOrWhiteSpace(DialedNumber)) 
            {
                ShowStatus("Enter a number first.");
                return;
            }
            
            string num = DialedNumber; 
            if (!num.StartsWith("C", StringComparison.OrdinalIgnoreCase))
            {
                num = "C" + num;
            }

            var cmd = new { cmd = "CALL", number = num };
            string json = JsonSerializer.Serialize(cmd);
            DevicePort.SerialPortInterface.Write(json + "\n");
            
            ShowStatus($"Calling {num}...");
        }

        private void PickUpCall()
        {
            var cmd = new { cmd = "PICK_UP" };
            DevicePort.SerialPortInterface.Write(JsonSerializer.Serialize(cmd) + "\n");
            ShowStatus("Picked Up");
            CloseIncomingCallDialog();
        }

        private void HangUpCall()
        {
            var cmd = new { cmd = "HANG_UP" };
            DevicePort.SerialPortInterface.Write(JsonSerializer.Serialize(cmd) + "\n");
            ShowStatus("Call Ended");
            CloseIncomingCallDialog();
        }

        private void ShowIncomingCallDialog()
        {
             Dispatcher.UIThread.InvokeAsync(() => 
             {
                Debug.WriteLine("[IntercomVM] ShowIncomingCallDialog Triggered - Resetting Watchdog");
                StartRingingWatchdog(); // Reset timer on every RINGING command

                // SAFETY: If we are already in a call, do not show the incoming call popup.
                if (IsCallActive)
                {
                    Debug.WriteLine("[IntercomVM] Call already active. Ignoring RINGING popup.");
                    return;
                }

                if (_incomingCallDialog == null || !_incomingCallDialog.IsVisible)
                {
                    _incomingCallDialog = new IncomingCallDialog
                    {
                        DataContext = this
                    };
                    _incomingCallDialog.Closed += (s, e) => _incomingCallDialog = null;
                    
                    if (App.Current.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                    {
                         _incomingCallDialog.Show(desktop.MainWindow);
                    }
                    else
                    {
                         _incomingCallDialog.Show();
                    }
                }
             });
        }

        private void CloseIncomingCallDialog()
        {
            _incomingCallDialog?.Close();
            _incomingCallDialog = null;
        }

        private void LoadContacts()
        {
            try
            {
                var dbContacts = DevicePort.GetContacts();
                SharedContacts.Clear();
                foreach (var c in dbContacts)
                {
                    SharedContacts.Add(new Contact { Id = c.Id, Name = c.Name, Number = c.Number });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading contacts: {ex.Message}");
            }
        }

        private async void DeleteContact(object? parameter)
        {
            if (parameter is Contact contact)
            {
                bool confirmed = true;
                if (OnRequestDeleteConfirmation != null)
                {
                    confirmed = await OnRequestDeleteConfirmation.Invoke(contact);
                }

                if (confirmed)
                {
                    DevicePort.DeleteFromPhonebook(contact.Id);
                    LoadContacts();
                }
            }
        }

        public IntercomDialogViewModel Self => this;
    }
}