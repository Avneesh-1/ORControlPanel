using Avalonia.Threading;
using ORControlPanelNew.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Text.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ORControlPanelNew.ViewModels.Temperature
{
    public class TemperatureViewModel : ReactiveObject
    {
        private readonly IAlertService _alertService;

        // Hidden actual values from hardware
        [Reactive] public string ActualTemperature { get; set; } = "25.0";
        [Reactive] public string ActualHumidity { get; set; } = "50.0";

        // Display values (Red during manual override)
        [Reactive] public string DisplayTemperature { get; set; } = "25.0";
        [Reactive] public string DisplayHumidity { get; set; } = "50.0";
        [Reactive] public string TemperatureColor { get; set; } = "#1A1A1A"; 
        [Reactive] public string HumidityColor { get; set; } = "#1A1A1A";

        // Other status properties
        [Reactive] public string Voltage { get; set; } = "0.0";
        [Reactive] public string Current { get; set; } = "0.0";
        [Reactive] public string TransformerStatus { get; set; } = "OK";
        [Reactive] public string FireStatus { get; set; } = "OFF";
        [Reactive] public string UpsStatus { get; set; } = "OFF";
        [Reactive] public bool IsUpsOn { get; set; } = false;
        [Reactive] public string AirDiffPressure { get; set; } = "0.0";

        private decimal? _targetTemp = null;
        private decimal? _targetHumd = null;
        private bool _isTempSynced = true;
        private bool _isHumdSynced = true;
        private DateTime _lastTempInteraction = DateTime.MinValue;
        private DateTime _lastHumdInteraction = DateTime.MinValue;
        private const int OVERRIDE_DURATION_MS = 2000;

        private CancellationTokenSource? _TempCts;
        private CancellationTokenSource? _HumdCts;
        public ICommand IncTempCommand { get; }
        public ICommand DecTempCommand { get; }
        public ICommand IncHumdCommand { get; }
        public ICommand DecHumdCommand { get; }

        public TemperatureViewModel(IAlertService alertService)
        {
            _alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));

            DevicePort.DataProcessor.OnTemperatureUpdated += (temp) =>
            {
                Log($"Received OnTemperatureUpdated: temp={temp}");
                _TempCts?.Cancel();
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ActualTemperature = temp;
                    // Only update display if we are outside the 2-second manual override window
                    if (DateTime.Now - _lastTempInteraction > TimeSpan.FromMilliseconds(OVERRIDE_DURATION_MS))
                    {
                        DisplayTemperature = temp;
                        TemperatureColor = "#1A1A1A";

                        // Sync Logic:
                        // 1. If Synced: Target tracks Actual.
                        // 2. If Not Synced: Check if Actual has reached Target. If so, Re-Sync.
                        if (decimal.TryParse(temp, CultureInfo.InvariantCulture, out decimal sensorVal))
                        {
                            // Initialize on first reading if null
                            if (_targetTemp == null || _isTempSynced)
                            {
                                _targetTemp = sensorVal;
                            }
                            else if (sensorVal == _targetTemp)
                            {
                                _isTempSynced = true;
                                // Log("Temperature Re-Synced to Hardware");
                            }
                        }
                    }
                });
            };

            DevicePort.DataProcessor.onTempUpdatedByController += (recievedByController) =>
            {
                Log($"Received onTempUpdatedByController: recievedByController={recievedByController}");
                _TempCts?.Cancel();
            };

            DevicePort.DataProcessor.onHumdUpdatedByController += (recievedByController) =>
            {
                Log($"Received onHumdUpdatedByController: recievedByController={recievedByController}");
                _HumdCts?.Cancel();
            };

            DevicePort.DataProcessor.OnHumidityUpdated += (humidity) =>
            {
                Log($"Received OnHumidityUpdated: humidity={humidity}");
                _HumdCts?.Cancel();
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ActualHumidity = humidity;
                    // Only update display if we are outside the 2-second manual override window
                    if (DateTime.Now - _lastHumdInteraction > TimeSpan.FromMilliseconds(OVERRIDE_DURATION_MS))
                    {
                        DisplayHumidity = humidity;
                        HumidityColor = "#1A1A1A";
                        
                        // Sync Logic
                        if (decimal.TryParse(humidity, CultureInfo.InvariantCulture, out decimal sensorVal))
                        {
                            // Initialize on first reading if null
                            if (_targetHumd == null || _isHumdSynced)
                            {
                                _targetHumd = sensorVal;
                            }
                            else if (sensorVal == _targetHumd)
                            {
                                _isHumdSynced = true;
                            }
                        }
                    }
                });
            };

            DevicePort.DataProcessor.OnTransformerUpdated += (voltage, current, isError) =>
            {
                Log($"Received OnTransformerUpdated: voltage={voltage}, current={current}, isError={isError}");
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Voltage = voltage;
                    Current = current;
                    TransformerStatus = isError ? "ERROR" : "OK";
                });
            };

            DevicePort.DataProcessor.onAirDiffPressureUpdated += (adp) =>
            {
                Log($"Received onAirDiffPressureUpdated: adp={adp}");
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    AirDiffPressure = adp;
                });
            };

            DevicePort.DataProcessor.OnFireStatusUpdated += (isActive) =>
            {
                Log($"Received OnFireStatusUpdated: isActive={isActive}");
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    FireStatus = isActive ? "ON" : "OFF";
                });
            };

            DevicePort.DataProcessor.OnUpsStatusUpdated += (isOn) =>
            {
                Log($"Received OnUpsStatusUpdated: isOn={isOn}");
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    UpsStatus = isOn ? "ON" : "OFF";
                    IsUpsOn = isOn;
                });
            };

            IncTempCommand = ReactiveCommand.Create(IncTemp);
            DecTempCommand = ReactiveCommand.Create(DecTemp);
            IncHumdCommand = ReactiveCommand.Create(IncHumd);
            DecHumdCommand = ReactiveCommand.Create(DecHumd);
        }

        private void SendTempCommand(decimal temp)
        {
            try
            {
                // decimal currentHumd = 0;
                // decimal.TryParse(ActualHumidity, out currentHumd);

                var cmd = new 
                { 
                    cmd = "SET_AHU", 
                    temp_set = temp, 
                    humi_set = _targetHumd ?? 0 // Safe fallback, though updates shouldn't happen if null
                };
                string jsonCmd = JsonSerializer.Serialize(cmd);
                Log($"Sending temperature command: {jsonCmd}");
                DevicePort.SerialPortInterface.Write(jsonCmd);
            }
            catch (Exception ex)
            {
                Log($"Error sending temperature command: {ex.Message}");
            }
        }

        private void SendHumdCommand(decimal humd)
        {
            try
            {
                // decimal currentTemp = 0;
                // decimal.TryParse(ActualTemperature, out currentTemp);

                var cmd = new 
                { 
                    cmd = "SET_AHU", 
                    temp_set = _targetTemp ?? 0, // Safe fallback
                    humi_set = humd 
                };
                string jsonCmd = JsonSerializer.Serialize(cmd);
                Log($"Sending humidity command: {jsonCmd}");
                DevicePort.SerialPortInterface.Write(jsonCmd);
            }
            catch (Exception ex)
            {
                Log($"Error sending humidity command: {ex.Message}");
            }
        }

        private void IncTemp()
        {
            if (_targetTemp == null) return; // Wait for hardware init

            _lastTempInteraction = DateTime.Now;
            _isTempSynced = false; // Break sync on interaction
            _targetTemp += 1;
            DisplayTemperature = _targetTemp.Value.ToString("0.0");
            TemperatureColor = "Red";
            SendTempCommand(_targetTemp.Value);

            Task.Delay(OVERRIDE_DURATION_MS).ContinueWith(_ => 
            {
                if (DateTime.Now - _lastTempInteraction >= TimeSpan.FromMilliseconds(OVERRIDE_DURATION_MS))
                {
                    Dispatcher.UIThread.Post(() => 
                    {
                        DisplayTemperature = ActualTemperature; // Revert to hardware value
                        TemperatureColor = "#1A1A1A";
                    });
                }
            });
        }

        private void DecTemp()
        {
            if (_targetTemp == null) return; // Wait for hardware init

            _lastTempInteraction = DateTime.Now;
            _isTempSynced = false; // Break sync on interaction
            _targetTemp -= 1;
            DisplayTemperature = _targetTemp.Value.ToString("0.0");
            TemperatureColor = "Red";
            SendTempCommand(_targetTemp.Value);

            Task.Delay(OVERRIDE_DURATION_MS).ContinueWith(_ => 
            {
                if (DateTime.Now - _lastTempInteraction >= TimeSpan.FromMilliseconds(OVERRIDE_DURATION_MS))
                {
                    Dispatcher.UIThread.Post(() => 
                    {
                        DisplayTemperature = ActualTemperature; // Revert to hardware value
                        TemperatureColor = "#1A1A1A";
                    });
                }
            });
        }

        private void IncHumd()
        {
            if (_targetHumd == null) return; // Wait for hardware init

            _lastHumdInteraction = DateTime.Now;
            _isHumdSynced = false; // Break sync on interaction
            _targetHumd += 1;
            DisplayHumidity = _targetHumd.Value.ToString("0.0");
            HumidityColor = "Red";
            SendHumdCommand(_targetHumd.Value);

            Task.Delay(OVERRIDE_DURATION_MS).ContinueWith(_ => 
            {
                if (DateTime.Now - _lastHumdInteraction >= TimeSpan.FromMilliseconds(OVERRIDE_DURATION_MS))
                {
                    Dispatcher.UIThread.Post(() => 
                    {
                        DisplayHumidity = ActualHumidity; // Revert to hardware value
                        HumidityColor = "#1A1A1A";
                    });
                }
            });
        }

        private void DecHumd()
        {
            if (_targetHumd == null) return; // Wait for hardware init

            _lastHumdInteraction = DateTime.Now;
            _isHumdSynced = false; // Break sync on interaction
            _targetHumd -= 1;
            DisplayHumidity = _targetHumd.Value.ToString("0.0");
            HumidityColor = "Red";
            SendHumdCommand(_targetHumd.Value);

            Task.Delay(OVERRIDE_DURATION_MS).ContinueWith(_ => 
            {
                if (DateTime.Now - _lastHumdInteraction >= TimeSpan.FromMilliseconds(OVERRIDE_DURATION_MS))
                {
                    Dispatcher.UIThread.Post(() => 
                    {
                        DisplayHumidity = ActualHumidity; // Revert to hardware value
                        HumidityColor = "#1A1A1A";
                    });
                }
            });
        }

        private static void Log(string message)
        {
            Debug.WriteLine($"[TemperatureViewModel] {message}");
        }
    }
}