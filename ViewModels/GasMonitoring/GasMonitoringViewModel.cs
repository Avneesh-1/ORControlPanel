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
//using NAudio.Wave;
using System.Reflection;

namespace ORControlPanelNew.ViewModels.GasMonitoring
{
    public class GasMonitoringViewModel : ReactiveObject
    {
        private ObservableCollection<GasStatus> _gases = new();
        private readonly Guid _instanceId = Guid.NewGuid();
        private readonly IAlertService _alertService;
        //private WaveOutEvent? _waveOut;
        //private WaveFileReader? _waveReader;
        //private bool _isDisposed = false;
        //private bool _isAudioPlaying = false;

        public ObservableCollection<GasStatus> Gases
        {
            get => _gases;
            set => this.RaiseAndSetIfChanged(ref _gases, value);
        }


        [Reactive] public bool GeneralGasAlert { get; set; } = false;
        [Reactive] public string Voltage { get; set; } = "0.0";
        [Reactive] public string Current { get; set; } = "0.0";
        [Reactive] public string TransformerStatus { get; set; } = "OK";
        [Reactive] public string FireStatus { get; set; } = "OFF";
        [Reactive] public string UpsStatus { get; set; } = "OFF";
        [Reactive] public bool IsUpsOn { get; set; } = false;
        [Reactive] public bool IsHepaGood { get; set; } = true;

        public ReactiveCommand<Unit, Unit> SimulateDataCommand { get; }

        public GasMonitoringViewModel(IAlertService alertService)
        {
            _alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));

            try
            {
                // Initialize NAudio
                var soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "digital-alarm-buzzer-992.wav");
                if (!File.Exists(soundPath))
                {
                    Log($"Audio file not found: {soundPath}");
                    throw new FileNotFoundException("Alert sound file not found.", soundPath);
                }

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

                InitializeGases();
                Log("INIT");

                // Event handlers
                DevicePort.DataProcessor.OnGasPressureUpdated += (gasName, pressure) =>
                {
                    Log($"Received OnGasPressureUpdated: gasName={gasName}, pressure={pressure}");
                    Dispatcher.UIThread.InvokeAsync(() => UpdateGasPressure(gasName, pressure));
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

                DevicePort.DataProcessor.OnHepaStatusUpdated += (isBad) =>
                {
                    Log($"Received OnHepaStatusUpdated: isBad={isBad}");
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        IsHepaGood = !isBad;
                        if (isBad)
                        {
                            _alertService.ShowAlert("Alert: HEPA Filter needs attention!");
                        }
                    });
                };

                DevicePort.DataProcessor.OnGasAlertUpdated += (gasName, isAlert) =>
                {
                    if (gasName == "General Gas Pressure")
                    {
                        Log($"Received OnGasAlertUpdated: gasName={gasName}, isAlert={isAlert}");
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            GeneralGasAlert = isAlert;
                            _alertService.ShowAlert("General Gas Pressure Alert");
                            //UpdateAudioPlayback(); // Trigger audio for GeneralGasAlert
                        });
                    }
                    Log($"Received OnGasAlertUpdated: gasName={gasName}, isAlert={isAlert}");
                    Dispatcher.UIThread.InvokeAsync(() => UpdateGasAlert(gasName, isAlert));
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
                if (isAlert)
                {
                    _alertService.ShowAlert($"Alert: Abnormal Gas : {gas.Name} !");
                }
                //UpdateAudioPlayback();
                Log($"After alert update: {gas.Name} IsAlert={gas.IsAlert}");
                this.RaisePropertyChanged(nameof(Gases));
            }
            else
            {
                Log($"Gas {gasName} not found in Gases collection.");
            }
        }

        //private void UpdateAudioPlayback()
        //{
        //    bool shouldPlay = Gases.Any(g => g.IsAlert);

        //    Dispatcher.UIThread.InvokeAsync(() =>
        //    {
        //        if (shouldPlay && !_isAudioPlaying)
        //        {
        //            _waveReader.Position = 0;
        //            _waveOut.Play();
        //            _isAudioPlaying = true;
        //            Log("Started audio playback for alert condition.");
        //        }
        //        else if (!shouldPlay && _isAudioPlaying)
        //        {
        //            _waveOut.Stop();
        //            _isAudioPlaying = false;
        //            Log("Stopped audio playback; no alert conditions active.");
        //        }
        //    });
        //}

        private void SimulateSerialData()
        {
            try
            {
                // Simulate gas pressure updates
                var random = new Random();
                var gases = new[] { "O₂", "N₂O", "CO₂", "AIR 4", "AIR 7", "VAC" };
                var gasCodes = new[] { "RDGA", "RDGB", "RDGC", "RDGD", "RDGE", "RDGF" };

                for (int i = 0; i < gases.Length; i++)
                {
                    var pressure = random.Next(20, 80).ToString();
                    var command = $"{gasCodes[i]}${pressure}";
                    Log($"Simulating command: {command}");
                    DevicePort.DataProcessor.ProcessData(command);
                }

                // Simulate HEPA status update
                var hepaStatus = random.Next(0, 2) == 1;
                var hepaCommand = $"HFST${(hepaStatus ? "15" : "5")}";
                Log($"Simulating HEPA command: {hepaCommand}");
                DevicePort.DataProcessor.ProcessData(hepaCommand);

                Log("Simulation completed successfully.");
            }
            catch (Exception ex)
            {
                Log($"Error during simulation: {ex.Message}");
            }
        }



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

        private static void Log(string message)
        {
            Debug.WriteLine($"[GasMonitoringViewModel] {message}");
        }
    }



}