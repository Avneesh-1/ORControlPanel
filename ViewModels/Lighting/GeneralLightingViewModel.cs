using System.Windows.Input;
using ReactiveUI;
using System;
using Avalonia.Controls;
using ORControlPanelNew.Views.Lighting;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Data;
using System.Linq;

namespace ORControlPanelNew.ViewModels.Lighting
{
    public class GeneralLightingViewModel : ReactiveObject
    {
        private bool _isLightOn;
        private bool _isDialogOpen;
        private double _light1Intensity;
        private double _light2Intensity;
        private bool _isLight1On;
        private bool _isLight2On;

        public bool IsLightOn
        {
            get => _isLightOn;
            set => this.RaiseAndSetIfChanged(ref _isLightOn, value);
        }

        public bool IsDialogOpen
        {
            get => _isDialogOpen;
            set => this.RaiseAndSetIfChanged(ref _isDialogOpen, value);
        }

        public double Light1Intensity
        {
            get => _light1Intensity;
            set => this.RaiseAndSetIfChanged(ref _light1Intensity, value);
        }

        public double Light2Intensity
        {
            get => _light2Intensity;
            set => this.RaiseAndSetIfChanged(ref _light2Intensity, value);
        }

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

        public ICommand ToggleLightCommand { get; }
        public ICommand OpenDialogCommand { get; }
        public ICommand ToggleLight1Command { get; }
        public ICommand ToggleLight2Command { get; }
        public ICommand CloseDialogCommand { get; }

        public GeneralLightingViewModel()
        {
            // Fetch initial values from the database
            try
            {
                // Fetch data for "General Lights 1" and "General Lights 2"
                string paramList = "'General Lights 1','General Lights 2'";
                DataTable dt = DevicePort.ReadValueFromDb(paramList);

                // Process the DataTable to set initial values
                foreach (DataRow row in dt.Rows)
                {
                    string fieldName = row["FieldName"].ToString();
                    string valueStr = row["Value"].ToString();

                    if (double.TryParse(valueStr, out double value))
                    {
                        if (fieldName == "General Lights 1")
                        {
                            Light1Intensity = value;
                            IsLight1On = value > 0; // Light is on if intensity > 0
                            Debug.WriteLine($"Fetched Light1Intensity from DB: {Light1Intensity}, IsLight1On: {IsLight1On}");
                        }
                        else if (fieldName == "General Lights 2")
                        {
                            Light2Intensity = value;
                            IsLight2On = value > 0; // Light is on if intensity > 0
                            Debug.WriteLine($"Fetched Light2Intensity from DB: {Light2Intensity}, IsLight2On: {IsLight2On}");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Failed to parse value for {fieldName}: {valueStr}");
                    }
                }

                // If no data was found for a light, use default values
                if (dt.Rows.Cast<DataRow>().All(row => row["FieldName"].ToString() != "General Lights 1"))
                {
                    Debug.WriteLine("No data found for General Lights 1 in DB, using defaults");
                    Light1Intensity = 0;
                    IsLight1On = false;
                }
                if (dt.Rows.Cast<DataRow>().All(row => row["FieldName"].ToString() != "General Lights 2"))
                {
                    Debug.WriteLine("No data found for General Lights 2 in DB, using defaults");
                    Light2Intensity = 0;
                    IsLight2On = false;
                }

                // Optionally sync the hardware with the fetched values
                if (IsLight1On && Light1Intensity > 0)
                {
                    DevicePort.SerialPortInterface.Write("LITA" + (int)Light1Intensity);
                }
                if (IsLight2On && Light2Intensity > 0)
                {
                    DevicePort.SerialPortInterface.Write("LITB" + (int)Light2Intensity);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching initial lighting data from DB: {ex.Message}");
                // Fallback to default values
            Light1Intensity = 0;
            Light2Intensity = 0;
            IsLight1On = false;
            IsLight2On = false;
            }

            ToggleLightCommand = ReactiveCommand.Create(() =>
            {
                IsLightOn = !IsLightOn;
            });

            OpenDialogCommand = ReactiveCommand.Create(() =>
            {
                var dialog = new LightingDialog();
                dialog.Show();
            });

            ToggleLight1Command = ReactiveCommand.Create(ToggleLight1);

            ToggleLight2Command = ReactiveCommand.Create(ToggleLight2);

            CloseDialogCommand = ReactiveCommand.Create(() =>
            {
                IsDialogOpen = false;
            });


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
                       DevicePort.SerialPortInterface.Write("LITA" + intValue);
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
                        DevicePort.SerialPortInterface.Write("LITB" + intValue);
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
                        DevicePort.SerialPortInterface.Write("LITA" + value);
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
                        DevicePort.SerialPortInterface.Write("LITA" + "0");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error writing to serial port: {ex.Message}");
                    }

                    // Attempt to update database
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
                        DevicePort.SerialPortInterface.Write("LITB" + value);
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
                        DevicePort.SerialPortInterface.Write("LITB" + "0");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error writing to serial port: {ex.Message}");
                    }

                    // Attempt to update database
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