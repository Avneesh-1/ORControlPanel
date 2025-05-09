using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using ORControlPanelNew.Models.GasMonitoring;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ORControlPanelNew.ViewModels.GasMonitoring
{
    public class GasMonitoringViewModel : ReactiveObject
    {
        private ObservableCollection<GasStatus> _gases = new();

        public ObservableCollection<GasStatus> Gases
        {
            get => _gases;
            set => this.RaiseAndSetIfChanged(ref _gases, value);
        }

        [Reactive] public string GeneralGasPressureStatus { get; set; } = "Normal";
        [Reactive] public string Temperature { get; set; } = "0.0";
        [Reactive] public string AirDiffPressure { get; set; } = "0.0";
        [Reactive] public string Humidity { get; set; } = "0.0";
        [Reactive] public string Voltage { get; set; } = "0.0";
        [Reactive] public string Current { get; set; } = "0.0";
        [Reactive] public string TransformerStatus { get; set; } = "OK";
        [Reactive] public string FireStatus { get; set; } = "OFF";
        [Reactive] public string HepaStatus { get; set; } = "BAD";
        [Reactive] public string UpsStatus { get; set; } = "OFF";
        [Reactive] public bool IsUpsOn { get; set; } = false;

        public ReactiveCommand<Unit, Unit> SimulateDataCommand { get; }

        public GasMonitoringViewModel()
        {
            try
            {
                InitializeGases();
                Log("INIT");
               
                if (!DevicePort.SerialPortInterface.Initialize("COM5"))
                {
                    Log("Warning: Serial port initialization failed for COM5. Proceeding without serial communication.");
                }

                DevicePort.DataProcessor.OnGasPressureUpdated += (gasName, pressure) =>
                {
                    Log($"Received OnGasPressureUpdated: gasName={gasName}, pressure={pressure}");
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        UpdateGasPressure(gasName, pressure);
                    });
                };

                DevicePort.DataProcessor.OnGasAlertUpdated += (gasName, isAlert) =>
                {
                    Log($"Received OnGasAlertUpdated: gasName={gasName}, isAlert={isAlert}");
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        UpdateGasAlert(gasName, isAlert);
                    });
                };

                DevicePort.DataProcessor.OnGeneralGasAlertUpdated += (isAlert) =>
                {
                    Log($"Received OnGeneralGasAlertUpdated: isAlert={isAlert}");
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        GeneralGasPressureStatus = isAlert ? "Alert" : "Normal";
                    });
                };

                DevicePort.DataProcessor.OnTemperatureUpdated += (temp) =>
                {
                    Log($"Received OnTemperatureUpdated: temp={temp}");
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Temperature = temp;
                    });
                };

                DevicePort.DataProcessor.OnHumidityUpdated += (humidity) =>
                {
                    Log($"Received OnHumidityUpdated: humidity={humidity}");
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Humidity = humidity;
                    });
                };

                DevicePort.DataProcessor.OnTransformerUpdated += (voltage, current, isError) =>
                {
                    Log($"Received OnTransformerUpdated: voltage={voltage}, current={current}, isError={isError}");
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Voltage = voltage;
                        Current = current;
                        TransformerStatus = isError ? "ERROR" : "OK";
                    });
                };

                DevicePort.DataProcessor.OnFireStatusUpdated += (isActive) =>
                {
                    Log($"Received OnFireStatusUpdated: isActive={isActive}");
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        FireStatus = isActive ? "ON" : "OFF";
                    });
                };

                DevicePort.DataProcessor.OnHepaStatusUpdated += (isBad) =>
                {
                    Log($"Received OnHepaStatusUpdated: isBad={isBad}");
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        HepaStatus = isBad ? "BAD" : "GOOD";
                    });
                };

                DevicePort.DataProcessor.OnUpsStatusUpdated += (isOn) =>
                {
                    Log($"Received OnUpsStatusUpdated: isOn={isOn}");
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        UpsStatus = isOn ? "ON" : "OFF";
                        IsUpsOn = isOn;
                    });
                };


                DevicePort.DataProcessor.onAirDiffPressureUpdated += (adp) =>
                {
                    Log($"Received onAirDiffPressureUpdated: ={adp}");
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        AirDiffPressure = adp;
                    });
                };

                SimulateDataCommand = ReactiveCommand.Create(SimulateSerialData);
            }
            catch (Exception ex)
            {
                Log($"Failed to initialize GasMonitoringViewModel: {ex.Message}");
                throw;
            }
        }

        private void InitializeGases()
        {
            Gases = new ObservableCollection<GasStatus>
            {
                new GasStatus("O₂"),
                new GasStatus("N₂O"),
                new GasStatus("CO₂"),
                new GasStatus("AIR 4"),
                new GasStatus("AIR 7"),
                new GasStatus("VAC")
            };
            Log("Initialized Gases collection.");
        }

        private void UpdateGasPressure(string gasName, string pressure)
        {
            var gas = Gases.FirstOrDefault(g => g.Name == gasName);
            if (gas != null)
            {
                Log($"Before pressure update: {gas.Name} Pressure={gas.Pressure}");
                gas.Pressure = pressure;
                Log($"After pressure update: {gas.Name} Pressure={gas.Pressure}");
                this.RaisePropertyChanged(nameof(Gases));
            }
            else
            {
                Log($"Gas {gasName} not found in Gases collection.");
            }
        }

        private void UpdateGasAlert(string gasName, bool isAlert)
        {
            var gas = Gases.FirstOrDefault(g => g.Name == gasName);
            if (gas != null)
            {
                Log($"Before alert update: {gas.Name} IsAlert={gas.IsAlert}");
                gas.IsAlert = isAlert;
                Log($"After alert update: {gas.Name} IsAlert={gas.IsAlert}");
                this.RaisePropertyChanged(nameof(Gases));
            }
            else
            {
                Log($"Gas {gasName} not found in Gases collection.");
            }
        }

        private void SimulateSerialData()
        {
            string[] testData = new[]
            {
                "RDGA$3.5",    // O₂ pressure
                "ARDP$43",
                "ALGA",        // O₂ alert ON
                "RDGB$6.0",    // N₂O pressure
                "BLGB",        // N₂O alert OFF
                "RDGC$4.5",    // CO₂ pressure
                "ALGC",        // CO₂ alert ON
                "RDGD$9.0",    // AIR 4 pressure
                "BLGD",        // AIR 4 alert OFF
                "RDGE$7.0",    // AIR 7 pressure
                "ALGE",        // AIR 7 alert ON
                "RDGF$-0.2",   // VAC pressure
                "BLGF",        // VAC alert OFF
                "TEMP$24.0",   // Temperature
                "HUMD$50.0",   // Humidity
                "ITIDV220$C10$ST1", // Transformer
                "FRAM$20.0",   // Fire ON
                "HFST$8.0",    // HEPA GOOD
                "UPSS$1"       // UPS ON
            };

            foreach (var data in testData)
            {
                Log($"Simulating data: {data}");
                DevicePort.DataProcessor.ProcessData(data);
            }
        }

        private static void Log(string message)
        {
            Debug.WriteLine(message);
        }
    }
}