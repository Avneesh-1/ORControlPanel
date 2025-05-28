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
    public class OTLightsViewModel : ReactiveObject
    {
        private bool _isLight1On;
        private double _lightIntensity;
        private CancellationTokenSource _OTLight1Cts;

        public bool IsLight1On
        {
            get => _isLight1On;
            set => this.RaiseAndSetIfChanged(ref _isLight1On, value);
        }

        public double LightIntensity
        {
            get => _lightIntensity;
            set => this.RaiseAndSetIfChanged(ref _lightIntensity, value);
        }

        public ICommand ToggleLight1Command { get; }
        public ICommand OpenDialogCommand { get; }

        public OTLightsViewModel()
        {
            try
            {
                string paramList = "'OTLight1'";
                var dt = DevicePort.ReadValueFromDb(paramList);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    LightIntensity = Convert.ToDouble(row["Value"]);
                    IsLight1On = LightIntensity > 0;
                }
                else
                {
                    Debug.WriteLine("No data found for OTLight1 in DB, using defaults");
                    LightIntensity = 0;
                    IsLight1On = false;
                }

                if (IsLight1On && LightIntensity > 0)
                {
                    DevicePort.SerialPortInterface.Write("LITE" + (int)LightIntensity);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching initial OTLight1 data from DB: {ex.Message}");
                LightIntensity = 0;
                IsLight1On = false;
            }

            ToggleLight1Command = ReactiveCommand.Create(ToggleOTLight1);
            DevicePort.DataProcessor.onOTLight2Updated += onOTLight1Updated;






        }

        private void onOTLight1Updated(bool receivedByController)
        {
            try
            {
                _OTLight1Cts?.Cancel();

                string dbValue = receivedByController ? "10" : "0";
                DevicePort.UpdateValueToDb(dbValue, "OTLight1");
                IsLight1On = receivedByController;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating database: {ex.Message}");
            }
        }



        private void ToggleOTLight1()
        {
            try
            {
                // Cancel any existing timeout
                _OTLight1Cts?.Cancel();
                _OTLight1Cts = new CancellationTokenSource();
                var token = _OTLight1Cts.Token;

                bool turningOn = !IsLight1On;
                string command = turningOn ? "LITE$10" : "LITE$0";
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
                            DevicePort.UpdateValueToDb("0", "OTLight1");
                            IsLight1On = false;
                        });
                    }
                }, token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error in OT1 ToggleLight: {ex.Message}");
            }
        }
    }
}