using Avalonia.Threading;
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
    public class OTLight2ViewModel : ReactiveObject
    {
        private bool _isLightOn;
        private double _lightIntensity;
        private CancellationTokenSource _OTLight2Cts;

        public bool IsLightOn
        {
            get => _isLightOn;
            set => this.RaiseAndSetIfChanged(ref _isLightOn, value);
        }

        public double LightIntensity
        {
            get => _lightIntensity;
            set => this.RaiseAndSetIfChanged(ref _lightIntensity, value);
        }

        public ICommand ToggleLightCommand { get; }
        public ICommand OpenDialogCommand { get; }

        public OTLight2ViewModel()
        {
            try
            {
                string paramList = "'OTLight2'";
                var dt = DevicePort.ReadValueFromDb(paramList);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    LightIntensity = Convert.ToDouble(row["Value"]);
                    IsLightOn = LightIntensity > 0;
                }
                else
                {
                    Debug.WriteLine("No data found for OTLight2 in DB, using defaults");
                    LightIntensity = 0;
                    IsLightOn = false;
                }

                if (IsLightOn && LightIntensity > 0)
                {
                    DevicePort.SerialPortInterface.Write("LITF" + (int)LightIntensity);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching initial OTLight2 data from DB: {ex.Message}");
                LightIntensity = 0;
                IsLightOn = false;
            }

            ToggleLightCommand = ReactiveCommand.Create(ToggleOTLight2);
            DevicePort.DataProcessor.onOTLight2Updated += onOTLight2Updated;






        }

        private void onOTLight2Updated(bool receivedByController)
        {
            try
            {
                _OTLight2Cts?.Cancel();

                string dbValue = receivedByController ? "10" : "0";
                DevicePort.UpdateValueToDb(dbValue, "OTLight2");
                IsLightOn = receivedByController;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating database: {ex.Message}");
            }
        }



        private void ToggleOTLight2()
        {
            try
            {
                // Cancel any existing timeout
                _OTLight2Cts?.Cancel();
                _OTLight2Cts = new CancellationTokenSource();
                var token = _OTLight2Cts.Token;

                bool turningOn = !IsLightOn;
                string command = turningOn ? "LITF$10" : "LITF$0";
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
                        Debug.WriteLine("OT Light2 Light feedback timeout → assuming OFF");
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            DevicePort.UpdateValueToDb("0", "OTLight2");
                            IsLightOn = false;
                        });
                    }
                }, token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error in OT2 ToggleLight: {ex.Message}");
            }
        }
    }
} 