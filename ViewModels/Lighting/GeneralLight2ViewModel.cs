using System.Windows.Input;
using ReactiveUI;
using System;
using ORControlPanelNew.Views.Lighting;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Data;
using System.Linq;

namespace ORControlPanelNew.ViewModels.Lighting
{
    public class GeneralLight2ViewModel : ReactiveObject
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

        public GeneralLight2ViewModel()
        {
            try
            {
                // Fetch initial values from database
                string paramList = "'General Lights 2'";
                var dt = DevicePort.ReadValueFromDb(paramList);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    LightIntensity = Convert.ToDouble(row["Value"]);
                    IsLightOn = LightIntensity > 0;
                }
                else
                {
                    Debug.WriteLine("No data found for General Lights 2 in DB, using defaults");
                    LightIntensity = 0;
                    IsLightOn = false;
                }

                // Optionally sync the hardware with the fetched values
                if (IsLightOn && LightIntensity > 0)
                {
                    DevicePort.SerialPortInterface.Write("LITB" + (int)LightIntensity);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching initial lighting data from DB: {ex.Message}");
                // Fallback to default values
                LightIntensity = 0;
                IsLightOn = false;
            }

            ToggleLightCommand = ReactiveCommand.Create(() =>
            {
                IsLightOn = !IsLightOn;
            });

            OpenDialogCommand = ReactiveCommand.Create(() =>
            {
                var dialog = new GeneralLight2Dialog();
                dialog.Show();
            });

            this.WhenAnyValue(x => x.LightIntensity)
                .Where(_ => IsLightOn)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Subscribe(value =>
                {
                    try
                    {
                        int intValue = (int)value;
                        DevicePort.UpdateValueToDb(intValue.ToString(), "General Lights 2");
                        Debug.WriteLine($"Updated LightIntensity: {intValue}");
                        DevicePort.SerialPortInterface.Write("LITB" + intValue);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating LightIntensity: {ex.Message}, InnerException: {ex.InnerException?.Message}");
                    }
                });
        }
    }
} 