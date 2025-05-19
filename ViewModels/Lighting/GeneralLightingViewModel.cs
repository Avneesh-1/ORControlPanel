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
                            IsLight1On = value > 0; // Light is on if intensity > 0
                            Debug.WriteLine($"Fetched Light1Intensity from DB: {value}, IsLight1On: {IsLight1On}");
                        }
                        else if (fieldName == "General Lights 2")
                        {
                            IsLight2On = value > 0; // Light is on if intensity > 0
                            Debug.WriteLine($"Fetched Light2Intensity from DB: {value}, IsLight2On: {IsLight2On}");
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
                    IsLight1On = false;
                }
                if (dt.Rows.Cast<DataRow>().All(row => row["FieldName"].ToString() != "General Lights 2"))
                {
                    Debug.WriteLine("No data found for General Lights 2 in DB, using defaults");
                    IsLight2On = false;
                }

                // Optionally sync the hardware with the fetched values
                // (No intensity logic here anymore)
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching initial lighting data from DB: {ex.Message}");
                // Fallback to default values
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
        }

        private void ToggleLight1()
        {
            try
            {
                if (!IsLight1On)
                {
                    // Turn on the light (intensity logic removed)
                    try
                    {
                        DevicePort.SerialPortInterface.Write("LITA10"); // Default to 10 when turning on
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error writing to serial port: {ex.Message}");
                    }
                    try
                    {
                        DevicePort.UpdateValueToDb("10", "General Lights 1");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating database: {ex.Message}");
                    }
                    IsLight1On = true;
                }
                else
                {
                    // Turn off the light
                    try
                    {
                        DevicePort.SerialPortInterface.Write("LITA0");
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
                Debug.WriteLine($"Unexpected error in ToggleLight1: {ex.Message}");
            }
        }

        private void ToggleLight2()
        {
            try
            {
                if (!IsLight2On)
                {
                    // Turn on the light (intensity logic removed)
                    try
                    {
                        DevicePort.SerialPortInterface.Write("LITB10"); // Default to 10 when turning on
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error writing to serial port: {ex.Message}");
                    }
                    try
                    {
                        DevicePort.UpdateValueToDb("10", "General Lights 2");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating database: {ex.Message}");
                    }
                    IsLight2On = true;
                }
                else
                {
                    // Turn off the light
                    try
                    {
                        DevicePort.SerialPortInterface.Write("LITB0");
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
                Debug.WriteLine($"Unexpected error in ToggleLight2: {ex.Message}");
            }
        }
    }
} 