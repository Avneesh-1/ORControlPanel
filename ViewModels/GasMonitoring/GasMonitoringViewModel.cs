using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using ORControlPanelNew.Models.GasMonitoring;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ORControlPanelNew.Services;
using Avalonia.Threading;
using NAudio.Wave;
using System.Reflection;

namespace ORControlPanelNew.ViewModels.GasMonitoring
{
    public class GasMonitoringViewModel : ReactiveObject, IDisposable
    {
        private ObservableCollection<GasStatus> _gases = new();
        private readonly Guid _instanceId = Guid.NewGuid();
        private readonly IAlertService _alertService;
        private WaveOutEvent? _waveOut;
        private WaveFileReader? _waveReader;
        private bool _isDisposed = false;
        private bool _isAudioPlaying = false;

        public ObservableCollection<GasStatus> Gases
        {
            get => _gases;
            set => this.RaiseAndSetIfChanged(ref _gases, value);
        }

        
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

        public GasMonitoringViewModel(IAlertService alertService)
        {
            _alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));

            try
            {
                InitializeAudio();
                InitializeGases();
                Log("INIT");

                // Event handlers
                DevicePort.DataProcessor.OnGasPressureUpdated += (gasName, pressure) =>
                {
                    Log($"Received OnGasPressureUpdated: gasName={gasName}, pressure={pressure}");
                    Dispatcher.UIThread.InvokeAsync(() => UpdateGasPressure(gasName, pressure));
                };

                DevicePort.DataProcessor.OnGasAlertUpdated += (gasName, isAlert) =>
                {
                    Log($"Received OnGasAlertUpdated: gasName={gasName}, isAlert={isAlert}");
                    Dispatcher.UIThread.InvokeAsync(() => UpdateGasAlert(gasName, isAlert));
                };

                DevicePort.DataProcessor.OnTemperatureUpdated += (temp) =>
                {
                    Log($"Received OnTemperatureUpdated: temp={temp}");
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        DevicePort.UpdateValueToDb(temp, "Temperature");
                        Temperature = temp;
                    });
                };

                DevicePort.DataProcessor.OnHumidityUpdated += (humidity) =>
                {
                    Log($"Received OnHumidityUpdated: humidity={humidity}");
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        DevicePort.UpdateValueToDb(humidity, "Humidity");
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

                DevicePort.DataProcessor.OnFireStatusUpdated += (isActive) =>
                {
                    Log($"Received OnFireStatusUpdated: isActive={isActive}");
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        FireStatus = isActive ? "ON" : "OFF";
                        if (isActive)
                        {
                            _alertService.ShowAlert("Alert: Fire detected!");
                        }
                    });
                };

                DevicePort.DataProcessor.OnHepaStatusUpdated += (isBad) =>
                {
                    Log($"Received OnHepaStatusUpdated: isBad={isBad}");
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        HepaStatus = isBad ? "BAD" : "GOOD";
                        UpdateAudioPlayback();
                        if (isBad)
                        {
                            _alertService.ShowAlert("Alert: HEPA filter status is BAD!");
                        }
                    });
                };

                DevicePort.DataProcessor.OnUpsStatusUpdated += (isOn) =>
                {
                    Log($"Received OnUpsStatusUpdated: isOn={isOn}");
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        UpsStatus = isOn ? "ON" : "OFF";
                        IsUpsOn = isOn;
                        UpdateAudioPlayback();
                        if (!isOn)
                        {
                            _alertService.ShowAlert("Alert: UPS is OFF!");
                        }
                    });
                };

                DevicePort.DataProcessor.onAirDiffPressureUpdated += (adp) =>
                {
                    Log($"Received onAirDiffPressureUpdated: adp={adp}");
                    Dispatcher.UIThread.InvokeAsync(() => AirDiffPressure = adp);
                };

                SimulateDataCommand = ReactiveCommand.Create(SimulateSerialData);
            }
            catch (Exception ex)
            {
                Log($"Failed to initialize GasMonitoringViewModel: {ex.Message}");
                throw;
            }
        }

        private void InitializeAudio()
        {
            try
            {
                string[] possiblePaths = new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "digital-alarm-buzzer-992.wav"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Sounds", "digital-alarm-buzzer-992.wav"),
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "Assets", "Sounds", "digital-alarm-buzzer-992.wav")
                };

                string? soundPath = null;
                foreach (string path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        soundPath = path;
                        break;
                    }
                }

                if (soundPath == null)
                {
                    Log("Warning: Alert sound file not found. Audio alerts will be disabled.");
                    return;
                }

                _waveReader = new WaveFileReader(soundPath);
                var loopStream = new LoopStream(_waveReader);
                _waveOut = new WaveOutEvent();
                _waveOut.Init(loopStream);
                _waveOut.PlaybackStopped += (s, e) =>
                {
                    if (!_isDisposed && _isAudioPlaying)
                    {
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Log("Audio playback stopped unexpectedly, restarting.");
                            _waveOut?.Play();
                        });
                    }
                };
            }
            catch (Exception ex)
            {
                Log($"Warning: Failed to initialize audio: {ex.Message}. Audio alerts will be disabled.");
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
            }
        }

        private void UpdateGasAlert(string gasName, bool isAlert)
        {
            var gas = Gases.FirstOrDefault(g => g.Name == gasName);
            if (gas != null)
            {
                gas.IsAlert = isAlert;
                UpdateAudioPlayback();
                if (isAlert)
                {
                    _alertService.ShowAlert($"Alert: {gasName} pressure is out of range!");
                }
            }
        }

        private void UpdateAudioPlayback()
        {
            bool shouldPlay = Gases.Any(g => g.IsAlert) || UpsStatus == "OFF" || HepaStatus == "BAD";

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (shouldPlay && !_isAudioPlaying && _waveOut != null)
                {
                    _waveOut.Play();
                    _isAudioPlaying = true;
                    Log("Started audio playback due to alert condition.");
                }
                else if (!shouldPlay && _isAudioPlaying && _waveOut != null)
                {
                    _waveOut.Stop();
                    _isAudioPlaying = false;
                    Log("Stopped audio playback; no alert conditions active.");
                }
            });
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
                "RDGD$9.0",    // AIR 4 pressure
                "BLGD",        // AIR 4 alert OFF
                "RDGE$7.0",    // AIR 7 pressure
                "RDGF$-0.2",   // VAC pressure
                "BLGF",        // VAC alert OFF
                "TEMP$54.0",   // Temperature
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

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            try
            {
                _waveOut?.Stop();
                _waveOut?.Dispose();
                _waveReader?.Dispose();
            }
            catch (Exception ex)
            {
                Log($"Error disposing audio resources: {ex.Message}");
            }
        }

        private static void Log(string message)
        {
            Debug.WriteLine($"[GasMonitoringViewModel] {message}");
        }
    }

    public class LoopStream : WaveStream
    {
        private readonly WaveStream _sourceStream;

        public LoopStream(WaveStream sourceStream)
        {
            _sourceStream = sourceStream;
            EnableLooping = true;
        }

        public bool EnableLooping { get; set; }

        public override WaveFormat WaveFormat => _sourceStream.WaveFormat;

        public override long Length => _sourceStream.Length;

        public override long Position
        {
            get => _sourceStream.Position;
            set => _sourceStream.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                int bytesRead = _sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    if (_sourceStream.Position == 0 || !EnableLooping)
                    {
                        break;
                    }
                    _sourceStream.Position = 0;
                }
                totalBytesRead += bytesRead;
            }

            return totalBytesRead;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _sourceStream.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}