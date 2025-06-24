using Avalonia.Threading;
using ORControlPanelNew.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Diagnostics;
using System.Globalization;
//using NAudio.Wave;
using System.IO;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ORControlPanelNew.ViewModels.Temperature
{
    public class TemperatureViewModel : ReactiveObject
    {
        //private readonly WaveOutEvent _waveOut;
        //private readonly WaveFileReader _waveReader;
        //private bool _isDisposed = false;
        //private bool _isAudioPlaying = false;
        //private bool _wasAlertTriggered = false;

        private readonly IAlertService _alertService;
        [Reactive] public string Temperature { get; set; } = "0.0";
        [Reactive] public string Humidity { get; set; } = "0.0";
        [Reactive] public string Voltage { get; set; } = "0.0";
        [Reactive] public string Current { get; set; } = "0.0";
        [Reactive] public string TransformerStatus { get; set; } = "OK";
        [Reactive] public string FireStatus { get; set; } = "OFF";
        [Reactive] public string UpsStatus { get; set; } = "OFF";
        
        [Reactive] public bool IsUpsOn { get; set; } = false;
        [Reactive] public string AirDiffPressure { get; set; } = "0.0";

        private CancellationTokenSource _TempCts;
        private CancellationTokenSource _HumdCts;
        public ICommand IncTempCommand { get; }
        public ICommand DecTempCommand { get; }
        public ICommand IncHumdCommand { get; }
        public ICommand DecHumdCommand { get; }

        public TemperatureViewModel(IAlertService alertService)
        {
            _alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
            //var soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "digital-alarm-buzzer-992.wav");
            //if (!File.Exists(soundPath))
            //{
            //    Log($"Audio file not found: {soundPath}");
            //    throw new FileNotFoundException("Alert sound file not found.", soundPath);
            //}
            //_waveReader = new WaveFileReader(soundPath);
            //_waveOut = new WaveOutEvent();
            //_waveOut.Init(_waveReader);
            //_waveOut.PlaybackStopped += (s, e) =>
            //{
            //    Dispatcher.UIThread.InvokeAsync(() =>
            //    {
            //        _isAudioPlaying = false;
            //        Log("Audio playback stopped.");
            //    });
            //};

            

          

            DevicePort.DataProcessor.OnTemperatureUpdated += (temp) =>
            {
                Log($"Received OnTemperatureUpdated: temp={temp}");
                _TempCts?.Cancel();
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Temperature = temp;
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
                    Humidity = humidity;
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
                    //UpdateAudioPlayback();
                });
            };

            DevicePort.DataProcessor.OnUpsStatusUpdated += (isOn) =>
            {
                Log($"Received OnUpsStatusUpdated: isOn={isOn}");
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    UpsStatus = isOn ? "ON" : "OFF";
                    IsUpsOn = isOn;
                    //UpdateAudioPlayback();
                });
            };

            IncTempCommand = ReactiveCommand.Create(IncTemp);
            DecTempCommand = ReactiveCommand.Create(DecTemp);
            IncHumdCommand = ReactiveCommand.Create(IncHumd);
            DecHumdCommand = ReactiveCommand.Create(DecHumd);
        }

        //private void UpdateAudioPlayback()
        //{
        //    bool shouldPlay = UpsStatus == "OFF" || HepaStatus == "BAD" || GeneralGasAlert;
        //    Dispatcher.UIThread.InvokeAsync(() =>
        //    {
        //        if (shouldPlay && !_isAudioPlaying && !_wasAlertTriggered)
        //        {
        //            _waveReader.Position = 0;
        //            _waveOut.Play();
        //            _isAudioPlaying = true;
        //            _wasAlertTriggered = true;
        //            Log("Started audio playback for alert condition.");
        //        }
        //        else if (!shouldPlay && _isAudioPlaying)
        //        {
        //            _waveOut.Stop();
        //            _isAudioPlaying = false;
        //            _wasAlertTriggered = false;
        //            Log("Stopped audio playback; no alert conditions active.");
        //        }
        //        else if (!shouldPlay)
        //        {
        //            _wasAlertTriggered = false;
        //        }
        //    });
        //}

        //public void Dispose()
        //{
        //    if (_isDisposed)
        //        return;
        //    _isDisposed = true;
        //    try
        //    {
        //        _waveOut?.Stop();
        //        _waveOut?.Dispose();
        //        _waveReader?.Dispose();
        //    }
        //    catch (Exception ex)
        //    {
        //        Log($"Error disposing audio resources: {ex.Message}");
        //    }
        //}

        private void SendTempCommand(decimal temp)
        {
            try
            {
                string command = $"TEMP${temp}";
                Log($"Sending temperature command: {command}");
                DevicePort.SerialPortInterface.Write(command);
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
                string command = $"HUMD${humd}";
                Log($"Sending humidity command: {command}");
                DevicePort.SerialPortInterface.Write(command);
            }
            catch (Exception ex)
            {
                Log($"Error sending humidity command: {ex.Message}");
            }
        }

        private void IncTemp()
        {
            if (decimal.TryParse(Temperature, out decimal currentTemp))
            {
                decimal newTemp = currentTemp + 1;
                SendTempCommand(newTemp);
            }
        }

        private void DecTemp()
        {
            if (decimal.TryParse(Temperature, out decimal currentTemp))
            {
                decimal newTemp = currentTemp - 1;
                SendTempCommand(newTemp);
            }
        }

        private void IncHumd()
        {
            if (decimal.TryParse(Humidity, out decimal currentHumd))
            {
                decimal newHumd = currentHumd + 1;
                SendHumdCommand(newHumd);
            }
        }

        private void DecHumd()
        {
            if (decimal.TryParse(Humidity, out decimal currentHumd))
            {
                decimal newHumd = currentHumd - 1;
                SendHumdCommand(newHumd);
            }
        }

        private static void Log(string message)
        {
            Debug.WriteLine($"[TemperatureViewModel] {message}");
        }
    }
}