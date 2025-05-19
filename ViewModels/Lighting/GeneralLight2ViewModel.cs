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
        private bool _isLight2On;

        public bool IsLight2On
        {
            get => _isLight2On;
            set => this.RaiseAndSetIfChanged(ref _isLight2On, value);
        }

        public ICommand ToggleLight2Command { get; }


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
                    IsLight2On = Convert.ToDouble(row["Value"]) > 0;
                }
                else
                {
                    Debug.WriteLine("No data found for General Lights 2 in DB, using defaults");
                    IsLight2On = false;
                }

                // Optionally sync the hardware with the fetched values
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching initial lighting data from DB: {ex.Message}");
                // Fallback to default values
                IsLight2On = false;
            }

            ToggleLight2Command = ReactiveCommand.Create(ToggleGeneralLight2);
        }

            private void ToggleGeneralLight2()
        {
            try
            {
                if (!IsLight2On)
                {
                    // Turn on the light (intensity logic removed)
                    try
                    {
                        DevicePort.SerialPortInterface.Write("LITB" + 10); // Default to 10 when turning on
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
                        DevicePort.SerialPortInterface.Write("LITB" + 0);
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
                Debug.WriteLine($"Unexpected error in General lights 2 ToggleLight: {ex.Message}");
            }
        }
    }
    
} 