using System;
using System.Data;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using ORControlPanelNew.Views.Brightness;
using ReactiveUI;

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
            set => this.RaiseAndSetIfChanged(ref _generalLight1Intensity, value);
        }

        private double _generalLight2Intensity;
        public double GeneralLight2Intensity
        {
            get => _generalLight2Intensity;
            set => this.RaiseAndSetIfChanged(ref _generalLight2Intensity, value);
        }

        private double _laminarLightIntensity;
        public double LaminarLightIntensity
        {
            get => _laminarLightIntensity;
            set => this.RaiseAndSetIfChanged(ref _laminarLightIntensity, value);
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

                // If all lights are OFF, do not show the dialog
                if (!IsGeneralLight1On && !IsGeneralLight2On && !IsLaminarLightOn)
                {
                    // Optionally, show a message to the user here
                    return;
                }

                if (_brightnessDialog == null || !_brightnessDialog.IsVisible)
                {
                    _brightnessDialog = new BrightnessDialog
                    {
                        DataContext = this
                    };
                    _brightnessDialog.Closed += (_, __) => _brightnessDialog = null;
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
        }

        private void UpdateGeneral1(double value)
        {
            try
            {
                int v = (int)value;
                DevicePort.UpdateValueToDb(v.ToString(), "General Lights 1");
                Debug.WriteLine($"Updated GeneralLight1Intensity: {v}");
                DevicePort.SerialPortInterface.Write("LITA" + v);
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
                int v = (int)value;
                DevicePort.UpdateValueToDb(v.ToString(), "General Lights 2");
                Debug.WriteLine($"Updated GeneralLight2Intensity: {v}");
                DevicePort.SerialPortInterface.Write("LITB" + v);
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
                int v = (int)value;
                DevicePort.UpdateValueToDb(v.ToString(), "Laminar Light");
                Debug.WriteLine($"Updated LaminarLightIntensity: {v}");
                DevicePort.SerialPortInterface.Write("LITI" + v);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating LaminarLight: {ex}");
            }
        }

        public void LoadInitialValues()
        {
            try
            {
                _suppressUpdates = true;

                string paramList = "'General Lights 1','General Lights 2','Laminar Light'";
                DataTable dt = DevicePort.ReadValueFromDb(paramList);

                // Reset ON/OFF states
                IsGeneralLight1On = false;
                IsGeneralLight2On = false;
                IsLaminarLightOn = false;

                foreach (DataRow row in dt.Rows)
                {
                    var name = row["FieldName"].ToString();
                    var str = row["Value"].ToString();

                    if (!double.TryParse(str, out var val))
                        continue;

                    switch (name)
                    {
                        case "General Lights 1":
                            GeneralLight1Intensity = val;
                            IsGeneralLight1On = val > 0;
                            break;
                        case "General Lights 2":
                            GeneralLight2Intensity = val;
                            IsGeneralLight2On = val > 0;
                            break;
                        case "Laminar Light":
                            LaminarLightIntensity = val;
                            IsLaminarLightOn = val > 0;
                            break;
                    }
                }
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
