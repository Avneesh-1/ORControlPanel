using System;
using System.Text.Json;
using System.Data;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Threading;
using ORControlPanelNew.Views.Brightness;
using ReactiveUI;
using ORControlPanelNew;

namespace ORControlPanelNew.ViewModels.Brightness
{
    public class BrightnessViewModel : ReactiveObject
    {
        private BrightnessDialog _brightnessDialog;
        private bool _suppressUpdates;

        public ICommand OpenDialogCommand { get; }
        public ICommand CloseCommand { get; }

        private double _generalLight1Intensity;
        public double GeneralLight1Intensity
        {
            get => _generalLight1Intensity;
            set => this.RaiseAndSetIfChanged(ref _generalLight1Intensity, Math.Max(1, value));
        }

        private double _generalLight2Intensity;
        public double GeneralLight2Intensity
        {
            get => _generalLight2Intensity;
            set => this.RaiseAndSetIfChanged(ref _generalLight2Intensity, Math.Max(1, value));
        }

        private double _laminarLightIntensity;
        public double LaminarLightIntensity
        {
            get => _laminarLightIntensity;
            set => this.RaiseAndSetIfChanged(ref _laminarLightIntensity, Math.Max(1, value));
        }

        // Add ON/OFF state properties
        private bool _isGeneralLight1On;
        public bool IsGeneralLight1On
        {
            get => _isGeneralLight1On;
            set => this.RaiseAndSetIfChanged(ref _isGeneralLight1On, value);
        }

        private bool _isGeneralLight2On;
        public bool IsGeneralLight2On
        {
            get => _isGeneralLight2On;
            set => this.RaiseAndSetIfChanged(ref _isGeneralLight2On, value);
        }

        private bool _isLaminarLightOn;
        public bool IsLaminarLightOn
        {
            get => _isLaminarLightOn;
            set => this.RaiseAndSetIfChanged(ref _isLaminarLightOn, value);
        }

        public BrightnessViewModel()
        {
            // Wire up your reactive updates, but only when not suppressing:
            this.WhenAnyValue(x => x.GeneralLight1Intensity)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Where(_ => !_suppressUpdates)
                .Subscribe(UpdateGeneral1);

            this.WhenAnyValue(x => x.GeneralLight2Intensity)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Where(_ => !_suppressUpdates)
                .Subscribe(UpdateGeneral2);

            this.WhenAnyValue(x => x.LaminarLightIntensity)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Where(_ => !_suppressUpdates)
                .Subscribe(UpdateLaminar);

            // Command to open (or activate) a single dialog instance:
            OpenDialogCommand = ReactiveCommand.Create(() =>
            {
                // Reload fresh DB values every time you open/activate:
                LoadInitialValues();

                // BUG FIX: Removed check that prevented dialog from opening if all lights OFF.
                
                if (_brightnessDialog == null || !_brightnessDialog.IsVisible)
                {
                    _brightnessDialog = new BrightnessDialog
                    {
                        DataContext = this
                    };
                    _brightnessDialog.Closed += (sender, args) => _brightnessDialog = null;
                    _brightnessDialog.Show();
                }
                else
                {
                    _brightnessDialog.Activate();
                }
            });

            // Initial load (for the very first open)
            LoadInitialValues();

            CloseCommand = ReactiveCommand.Create(() =>
            {
                if (_brightnessDialog != null)
                {
                    _brightnessDialog.Close();
                }
            });

            // Subscribe to live hardware updates to keep sliders in sync
            DevicePort.DataProcessor.onGeneralLight1Updated += (isOn) => {
                Dispatcher.UIThread.InvokeAsync(() => {
                    _suppressUpdates = true;
                    IsGeneralLight1On = isOn;
                    GeneralLight1Intensity = SystemInfo.GeneralLight1Intensity;
                    _suppressUpdates = false;
                });
            };
            DevicePort.DataProcessor.onGeneralLight2Updated += (isOn) => {
                Dispatcher.UIThread.InvokeAsync(() => {
                    _suppressUpdates = true;
                    IsGeneralLight2On = isOn;
                    GeneralLight2Intensity = SystemInfo.GeneralLight2Intensity;
                    _suppressUpdates = false;
                });
            };
            DevicePort.DataProcessor.onLaminarLightUpdated += (isOn) => {
                Dispatcher.UIThread.InvokeAsync(() => {
                    _suppressUpdates = true;
                    IsLaminarLightOn = isOn;
                    LaminarLightIntensity = SystemInfo.LaminarLightIntensity;
                    _suppressUpdates = false;
                });
            };
        }

        private void UpdateGeneral1(double value)
        {
            try
            {
                if (!SystemInfo.GeneralLight1On)
                {
                    Debug.WriteLine("Brightness change ignored (General Light 1 is OFF)");
                    // Ensure local state matches global? 
                    // If user moved slider even though off, we ignore it.
                    return; 
                }

                int v = (int)value;
                // DevicePort.UpdateValueToDb(v.ToString(), "General Lights 1"); // Disabled
                Debug.WriteLine($"Updated GeneralLight1Intensity: {v}");
                
                var cmd = new { cmd = "SET_GEN1", state = "ON", val = v };
                string jsonCmd = JsonSerializer.Serialize(cmd);
                DevicePort.SerialPortInterface.Write(jsonCmd + "\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating GeneralLight1: {ex}");
            }
        }

        private void UpdateGeneral2(double value)
        {
            try
            {
                if (!SystemInfo.GeneralLight2On)
                {
                    Debug.WriteLine("Brightness change ignored (General Light 2 is OFF)");
                    return; 
                }

                int v = (int)value;
                Debug.WriteLine($"Updated GeneralLight2Intensity: {v}");
                
                var cmd = new { cmd = "SET_GEN2", state = "ON", val = v };
                string jsonCmd = JsonSerializer.Serialize(cmd);
                DevicePort.SerialPortInterface.Write(jsonCmd + "\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating GeneralLight2: {ex}");
            }
        }

        private void UpdateLaminar(double value)
        {
            try
            {
                if (!SystemInfo.LaminarLightOn)
                {
                    Debug.WriteLine("Brightness change ignored (Laminar Light is OFF)");
                    return; 
                }

                int v = (int)value;
                Debug.WriteLine($"Updated LaminarLightIntensity: {v}");
                
                var cmd = new { cmd = "SET_LAM", state = "ON", val = v };
                string jsonCmd = JsonSerializer.Serialize(cmd);
                DevicePort.SerialPortInterface.Write(jsonCmd + "\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating LaminarLight: {ex}");
            }
        }

        public void LoadInitialValues()
        {
            _suppressUpdates = true;
            try
            {
                // DB is disabled for settings, so we rely on Global State (SystemInfo) which tracks the live state
                
                // General Light 1
                // General Light 1
                IsGeneralLight1On = SystemInfo.GeneralLight1On;
                GeneralLight1Intensity = SystemInfo.GeneralLight1Intensity;
                
                // General Light 2
                IsGeneralLight2On = SystemInfo.GeneralLight2On;
                GeneralLight2Intensity = SystemInfo.GeneralLight2Intensity;
                
                // Laminar Light
                IsLaminarLightOn = SystemInfo.LaminarLightOn;
                LaminarLightIntensity = SystemInfo.LaminarLightIntensity;

                Debug.WriteLine($"BrightnessVM Loaded: Gen1={IsGeneralLight1On}({GeneralLight1Intensity}), Gen2={IsGeneralLight2On}({GeneralLight2Intensity})");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading brightness values: {ex}");
            }
            finally
            {
                _suppressUpdates = false;
            }
        }
    }
}