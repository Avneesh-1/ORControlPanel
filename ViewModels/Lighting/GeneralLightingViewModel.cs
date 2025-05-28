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
    public class GeneralLightingViewModel : ReactiveObject
    {
        private bool _isLightOn;

        public bool IsLightOn
        {
            get => _isLightOn;
            set => this.RaiseAndSetIfChanged(ref _isLightOn, value);
        }

        private CancellationTokenSource _GeneralLight1Cts;

        public ICommand ToggleLightCommand { get; }


        public GeneralLightingViewModel()
        {
            try
            {
                // Fetch initial values from database
                string paramList = "'General Lights 2'";
                var dt = DevicePort.ReadValueFromDb(paramList);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    IsLightOn = Convert.ToDouble(row["Value"]) > 0;
                }
                else
                {
                    Debug.WriteLine("No data found for General Lights 1 in DB, using defaults");
                    IsLightOn = false;
                }

                // Optionally sync the hardware with the fetched values
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching initial lighting data from DB: {ex.Message}");
                // Fallback to default values
                IsLightOn = false;
            }

            ToggleLightCommand = ReactiveCommand.Create(ToggleGeneralLight1);
            DevicePort.DataProcessor.onGeneralLight1Updated += onGeneralLight1Updated;
        }


        private void onGeneralLight1Updated(bool receivedByController)
        {
            try
            {
                _GeneralLight1Cts?.Cancel();

                string dbValue = receivedByController ? "10" : "0";
                DevicePort.UpdateValueToDb(dbValue, "General Lights 1");
                IsLightOn = receivedByController;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating database: {ex.Message}");
            }
        }


        private void ToggleGeneralLight1()
        {
            try
            {
                // Cancel any existing timeout
                _GeneralLight1Cts?.Cancel();
                _GeneralLight1Cts = new CancellationTokenSource();
                var token = _GeneralLight1Cts.Token;

                bool turningOn = !IsLightOn;
                string command = turningOn ? "LITA$10" : "LITA$0";
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
                        Debug.WriteLine("general Light1 Light feedback timeout → assuming OFF");
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            DevicePort.UpdateValueToDb("0", "General Lights 1");
                            IsLightOn = false;
                        });
                    }
                }, token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error in General Lights 1 ToggleLight: {ex.Message}");
            }
        }
    }

}