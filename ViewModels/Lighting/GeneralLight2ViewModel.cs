using Avalonia.Threading;
using ORControlPanelNew.Views.Lighting;
using ReactiveUI;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ORControlPanelNew.ViewModels.Lighting
{
    public class GeneralLight2ViewModel : ReactiveObject
    {
        private bool _isLight2On;

        public bool IsLight2On
        {
            get => _isLight2On;
            set => this.RaiseAndSetIfChanged(ref _isLight2On, value);
        }

        private CancellationTokenSource _GeneralLight2Cts;

        public ICommand ToggleLight2Command { get; }


        public GeneralLight2ViewModel()
        {
            try
            {
                // Fetch initial values from database
                string paramList = "'General Lights 2'";
                var dt = DevicePort.ReadValueFromDb(paramList);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    IsLight2On = Convert.ToDouble(row["Value"]) > 0;
                }
                else
                {
                    Debug.WriteLine("No data found for General Lights 2 in DB, using defaults");
                    IsLight2On = false;
                }

                // Optionally sync the hardware with the fetched values
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching initial lighting data from DB: {ex.Message}");
                // Fallback to default values
                IsLight2On = false;
            }

            ToggleLight2Command = ReactiveCommand.Create(ToggleGeneralLight2);
            DevicePort.DataProcessor.onGeneralLight2Updated += onGeneralLight2Updated;
        }


        private void onGeneralLight2Updated(bool receivedByController)
        {
            try
            {
                _GeneralLight2Cts?.Cancel();

                string dbValue = receivedByController ? "10" : "0";
                DevicePort.UpdateValueToDb(dbValue, "General Lights 2");
                IsLight2On = receivedByController;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating database: {ex.Message}");
            }
        }


        private void ToggleGeneralLight2()
        {
            try
            {
                // Cancel any existing timeout
                _GeneralLight2Cts?.Cancel();
                _GeneralLight2Cts = new CancellationTokenSource();
                var token = _GeneralLight2Cts.Token;

                bool turningOn = !IsLight2On;
                string command = turningOn ? "LITB$10" : "LITB$0";
                string dbValue = turningOn ? "10" : "0";

                try
                {
                    DevicePort.SerialPortInterface.Write(command);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error writing to serial port: {ex.Message}");
                }

                // Start a 3-second timeout. If no hardware feedback is received, assume OFF.
                Task.Delay(TimeSpan.FromSeconds(3), token).ContinueWith(t =>
                {
                    if (!t.IsCanceled)
                    {
                        Debug.WriteLine("general Light2 Light feedback timeout → assuming OFF");
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            DevicePort.UpdateValueToDb("0", "General Lights 2");
                            IsLight2On = false;
                        });
                    }
                }, token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error in General Lights2 ToggleLight: {ex.Message}");
            }
        }
    }
    
} 