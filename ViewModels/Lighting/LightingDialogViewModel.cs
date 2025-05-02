using ORControlPanelNew;
using ReactiveUI;
using System.Diagnostics;
using System;
using System.Reactive;
using System.Reactive.Linq;


namespace ORControlPanelNew.ViewModels.Lighting
{
    public class LightingDialogViewModel : ReactiveObject
    {
        private bool _isLight1On;
        public bool IsLight1On
        {
            get => _isLight1On;
            set => this.RaiseAndSetIfChanged(ref _isLight1On, value);
        }

        private double _light1Intensity;
        public double Light1Intensity
        {
            get => _light1Intensity;
            set => this.RaiseAndSetIfChanged(ref _light1Intensity, value);
        }

        private bool _isLight2On;
        public bool IsLight2On
        {
            get => _isLight2On;
            set => this.RaiseAndSetIfChanged(ref _isLight2On, value);
        }

        private double _light2Intensity;
        public double Light2Intensity
        {
            get => _light2Intensity;
            set => this.RaiseAndSetIfChanged(ref _light2Intensity, value);
        }

        public ReactiveCommand<Unit, Unit> ToggleLight1Command { get; }
        public ReactiveCommand<Unit, Unit> ToggleLight2Command { get; }

        public LightingDialogViewModel()
        {
            ToggleLight1Command = ReactiveCommand.Create(ToggleLight1);
            ToggleLight2Command = ReactiveCommand.Create(ToggleLight2);
            this.WhenAnyValue(x => x.Light1Intensity)
                .Where(_ => IsLight1On)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Subscribe(value =>
                {
                    try
                    {
                        int intValue = (int)value;

                        DevicePort.UpdateValueToDb(intValue.ToString(), "General Lights 1");

                        Debug.WriteLine($"Updated Light1Intensity: {intValue}");
                        DevicePort.SerialPortInterface.Write("L1" + intValue);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating Light1Intensity: {ex.Message}, InnerException: {ex.InnerException?.Message}");
                    }
                });

            this.WhenAnyValue(x => x.Light2Intensity)
                .Where(_ => IsLight2On)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Subscribe(value =>
                {
                    try
                    {
                        int intValue = (int)value;

                        DevicePort.UpdateValueToDb(intValue.ToString(), "General Lights 2");
                        Debug.WriteLine($"Updated Light2Intensity: {intValue}");
                        DevicePort.SerialPortInterface.Write("L2" + intValue);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating Light2Intensity: {ex.Message}, InnerException: {ex.InnerException?.Message}");
                    }
                });
        }

        
        
        
        private void ToggleLight1()
        {
            try
            {
                if (!IsLight1On)
                {
                    var value = (int)(Light1Intensity == 0 ? 10 : Light1Intensity);

                    // Attempt to write to serial port
                    try
                    {
                        DevicePort.SerialPortInterface.Write("L1" + value); // "L1" is like `trackBar1.Tag`
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error writing to serial port: {ex.Message}");
                        // Optionally handle the error (e.g., show a user notification)
                    }

                    // Attempt to update database
                    try
                    {
                        DevicePort.UpdateValueToDb(value.ToString(), "General Lights 1");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating database: {ex.Message}");
                        // Optionally handle the error (e.g., show a user notification)
                    }

                    Light1Intensity = value;
                    IsLight1On = true;
                }
                else
                {
                    // Turn off the light
                    try
                    {
                        DevicePort.SerialPortInterface.Write("L1" + "0");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error writing to serial port: {ex.Message}");
                    }

                    try
                    {
                        DevicePort.UpdateValueToDb("0", "General Lights 1");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating database: {ex.Message}");
                    }

                    IsLight1On = false;
                }
            }
            catch (Exception ex)
            {
                // Catch any unexpected exceptions
                Debug.WriteLine($"Unexpected error in ToggleLight1: {ex.Message}");
            }
        }




        private void ToggleLight2()
        {
            try
            {
                if (!IsLight2On)
                {
                    var value = (int)(Light2Intensity == 0 ? 10 : Light2Intensity);

                    // Attempt to write to serial port
                    try
                    {
                        DevicePort.SerialPortInterface.Write("L2" + value); // "L1" is like `trackBar1.Tag`
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error writing to serial port: {ex.Message}");
                        // Optionally handle the error (e.g., show a user notification)
                    }

                    // Attempt to update database
                    try
                    {
                        DevicePort.UpdateValueToDb(value.ToString(), "General Lights 2");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating database: {ex.Message}");
                        // Optionally handle the error (e.g., show a user notification)
                    }

                    Light2Intensity = value;
                    IsLight2On = true;
                }
                else
                {
                    // Turn off the light
                    try
                    {
                        DevicePort.SerialPortInterface.Write("L2" + "0");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error writing to serial port: {ex.Message}");
                    }

                    try
                    {
                        DevicePort.UpdateValueToDb("0", "General Lights 2");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating database: {ex.Message}");
                    }

                    IsLight2On = false;
                }
            }
            catch (Exception ex)
            {
                // Catch any unexpected exceptions
                Debug.WriteLine($"Unexpected error in ToggleLight2: {ex.Message}");
            }
        }

    }
}