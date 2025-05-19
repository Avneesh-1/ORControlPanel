using System.Windows.Input;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Data;
using System.Linq;

namespace ORControlPanelNew.ViewModels.Lighting
{
    public class OTLight2ViewModel : ReactiveObject
    {
        private bool _isLightOn;
        private double _lightIntensity;

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

            ToggleLightCommand = ReactiveCommand.Create(() =>
            {
                IsLightOn = !IsLightOn;
                // Optionally update hardware and DB here
                DevicePort.UpdateValueToDb(IsLightOn ? ((int)(LightIntensity == 0 ? 10 : LightIntensity)).ToString() : "0", "OTLight2");
                DevicePort.SerialPortInterface.Write("LITF" + (IsLightOn ? ((int)(LightIntensity == 0 ? 10 : LightIntensity)).ToString() : "0"));
            });

          

            this.WhenAnyValue(x => x.LightIntensity)
                .Where(_ => IsLightOn)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Subscribe(value =>
                {
                    try
                    {
                        int intValue = (int)value;
                        DevicePort.UpdateValueToDb(intValue.ToString(), "OTLight2");
                        Debug.WriteLine($"Updated OTLight2 Intensity: {intValue}");
                        DevicePort.SerialPortInterface.Write("LITF" + intValue);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating OTLight2 Intensity: {ex.Message}, InnerException: {ex.InnerException?.Message}");
                    }
                });
        }
    }
} 