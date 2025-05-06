using System.Windows.Input;
using ReactiveUI;
using System;
using ORControlPanelNew.Views.Lighting;
using System.Diagnostics;

namespace ORControlPanelNew.ViewModels.Lighting
{
    public class OTLightsViewModel : ReactiveObject
    {
        private bool _isLight1On;
        private bool _isLight2On;
        private bool _isLightOn;

        public bool IsLight1On
        {
            get => _isLight1On;
            set => this.RaiseAndSetIfChanged(ref _isLight1On, value);
        }

        public bool IsLight2On
        {
            get => _isLight2On;
            set => this.RaiseAndSetIfChanged(ref _isLight2On, value);
        }

        public bool IsLightOn
        {
            get => _isLightOn;
            set => this.RaiseAndSetIfChanged(ref _isLightOn, value);
        }

        public ICommand ToggleLight1Command { get; }
        public ICommand ToggleLight2Command { get; }
        public ICommand OpenDialogCommand { get; }

        public OTLightsViewModel()
        {
            ToggleLight1Command = ReactiveCommand.Create(ToggleOTLight1);

            ToggleLight2Command = ReactiveCommand.Create(ToggleOTLight2);

            OpenDialogCommand = ReactiveCommand.Create(() =>
            {
                var dialog = new OTLightsDialog();
                dialog.Show();
            });
        }

        private void ToggleOTLight1()
        {
            try
            {
                if (!IsLight1On)
                {
                    

                    // Attempt to write to serial port
                    try
                    {
                        DevicePort.SerialPortInterface.Write("LITE" + 1); // "L1" is like `trackBar1.Tag`
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error writing to serial port: {ex.Message}");
                        // Optionally handle the error (e.g., show a user notification)
                    }

                    // Attempt to update database
                    try
                    {
                        DevicePort.UpdateValueToDb("1", "OTLight1");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating database: {ex.Message}");
                        // Optionally handle the error (e.g., show a user notification)
                    }

                    
                    IsLight1On = true;
                }
                else
                {
                    // Turn off the light
                    try
                    {
                        DevicePort.SerialPortInterface.Write("LITE" + "0");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error writing to serial port: {ex.Message}");
                    }

                    try
                    {
                        DevicePort.UpdateValueToDb("0", "OTLight1");
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
                Debug.WriteLine($"Unexpected error in OTLight1: {ex.Message}");
            }
        }




        private void ToggleOTLight2()
        {
            try
            {
                if (!IsLight2On)
                {
                    

                    // Attempt to write to serial port
                    try
                    {
                        DevicePort.SerialPortInterface.Write("LITF" + 1); // "L1" is like `trackBar1.Tag`
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error writing to serial port: {ex.Message}");
                        // Optionally handle the error (e.g., show a user notification)
                    }

                    // Attempt to update database
                    try
                    {
                        DevicePort.UpdateValueToDb("1", "OTLight2");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating database: {ex.Message}");
                        // Optionally handle the error (e.g., show a user notification)
                    }

                   
                    IsLight2On = true;
                }
                else
                {
                    // Turn off the light
                    try
                    {
                        DevicePort.SerialPortInterface.Write("LITF" + "0");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error writing to serial port: {ex.Message}");
                    }

                    try
                    {
                        DevicePort.UpdateValueToDb("0", "OTLight2");
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
                Debug.WriteLine($"Unexpected error in OTLight2: {ex.Message}");
            }
        }
    }
} 