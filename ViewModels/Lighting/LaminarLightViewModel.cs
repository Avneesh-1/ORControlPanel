using System.Windows.Input;
using ReactiveUI;
using System;
using ORControlPanelNew.Views.Lighting;
using System.Diagnostics;
using System.Reactive.Linq;
namespace ORControlPanelNew.ViewModels.Lighting
{
    public class LaminarLightViewModel : ReactiveObject
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

        public LaminarLightViewModel()
        {
            ToggleLightCommand = ReactiveCommand.Create(ToggleLaminarLight);

            OpenDialogCommand = ReactiveCommand.Create(() =>
            {
                var dialog = new LaminarLightDialog();
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

                       DevicePort.UpdateValueToDb(intValue.ToString(), "Laminar Light");

                       Debug.WriteLine($"Updated Laminar Intensity: {intValue}");
                       DevicePort.SerialPortInterface.Write("LITI" + intValue);
                   }
                   catch (Exception ex)
                   {
                       Debug.WriteLine($"Error updating Laminar LightIntensity: {ex.Message}, InnerException: {ex.InnerException?.Message}");
                   }
               });


        }

        private void ToggleLaminarLight()
        {
            try
            {
                if (!IsLightOn)
                {
                    var value = (int)(LightIntensity == 0 ? 10 : LightIntensity);

                    // Attempt to write to serial port
                    try
                    {
                        DevicePort.SerialPortInterface.Write("LITI" + value); // "L1" is like `trackBar1.Tag`
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error writing to serial port: {ex.Message}");
                        // Optionally handle the error (e.g., show a user notification)
                    }

                    // Attempt to update database
                    try
                    {
                        DevicePort.UpdateValueToDb(value.ToString(), "Laminar Light");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating database: {ex.Message}");
                        // Optionally handle the error (e.g., show a user notification)
                    }

                    LightIntensity = value;
                    IsLightOn = true;
                }
                else
                {
                    // Turn off the light
                    try
                    {
                        DevicePort.SerialPortInterface.Write("LITI" + "0");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error writing to serial port: {ex.Message}");
                    }

                    try
                    {
                        DevicePort.UpdateValueToDb("0", "Laminar Light");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating database: {ex.Message}");
                    }

                    IsLightOn = false;
                }
            }
            catch (Exception ex)
            {
                // Catch any unexpected exceptions
                Debug.WriteLine($"Unexpected error in Laminar ToggleLight: {ex.Message}");
            }
        }




        

    }
} 