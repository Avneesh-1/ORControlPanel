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
    public class LaminarLightViewModel : ReactiveObject
    {
        private CancellationTokenSource _laminarLightCts;

        private bool _isLightOn;

        public bool IsLightOn
        {
            get => _isLightOn;
            set => this.RaiseAndSetIfChanged(ref _isLightOn, value);
        }

        public ICommand ToggleLightCommand { get; }
       

        public LaminarLightViewModel()
        {
            try
            {
                string paramList = "'Laminar Light'";
                DataTable dt = DevicePort.ReadValueFromDb(paramList);
                foreach (DataRow row in dt.Rows)
                {
                    string fieldName = row["FieldName"].ToString();
                    string valueStr = row["Value"].ToString();
                    if (double.TryParse(valueStr, out double value))
                    {
                        if (fieldName == "Laminar Light")
                        {
                            IsLightOn = value > 0;
                            Debug.WriteLine($"Fetched LightIntensity from DB: {value}, IsLight1On: {IsLightOn}");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Failed to parse value for {fieldName}: {valueStr}");
                    }
                }
                if (dt.Rows.Cast<DataRow>().All(row => row["FieldName"].ToString() != "Laminar Light"))
                {
                    Debug.WriteLine("No data found for Laminar Lights  in DB, using defaults");
                    IsLightOn = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching initial lighting data from DB: {ex.Message}");
                IsLightOn = false;
            }
            ToggleLightCommand = ReactiveCommand.Create(ToggleLaminarLight);
            DevicePort.DataProcessor.onLaminarLightUpdated += OnLaminarLightUpdated;

        }

        private void OnLaminarLightUpdated(bool receivedByController)
        {
            try
            {
                _laminarLightCts?.Cancel();

                string dbValue = receivedByController ? "10" : "0";
                DevicePort.UpdateValueToDb(dbValue, "Laminar Light");
                IsLightOn = receivedByController;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating database: {ex.Message}");
            }
        }

        private void ToggleLaminarLight()
        {
            try
            {
                // Cancel any existing timeout
                _laminarLightCts?.Cancel();
                _laminarLightCts = new CancellationTokenSource();
                var token = _laminarLightCts.Token;

                bool turningOn = !IsLightOn;
                string command = turningOn ? "LITI$10" : "LITI$0";
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
                        Debug.WriteLine("Laminar Light feedback timeout → assuming OFF");
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            DevicePort.UpdateValueToDb("0", "Laminar Light");
                            IsLightOn = false;
                        });
                    }
                }, token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error in Laminar ToggleLight: {ex.Message}");
            }
        }
    }
} 