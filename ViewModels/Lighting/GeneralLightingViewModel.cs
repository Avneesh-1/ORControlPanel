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
    public class GeneralLightingViewModel : ReactiveObject
    {
        private bool _isLightOn;

        public bool IsLightOn
        {
            get => _isLightOn;
            set => this.RaiseAndSetIfChanged(ref _isLightOn, value);
        }

        public ICommand ToggleLightCommand { get; }


        public GeneralLightingViewModel()
        {
            try
            {
                // Fetch initial values from database
                string paramList = "'General Lights 1'";
                var dt = DevicePort.ReadValueFromDb(paramList);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    IsLightOn = Convert.ToDouble(row["Value"]) > 0;
                }
                else
                {
                    Debug.WriteLine("No data found for General Lights 1 in DB, using defaults");
                    IsLightOn = false;
                }

                // Optionally sync the hardware with the fetched values
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching initial lighting data from DB: {ex.Message}");
                // Fallback to default values
                IsLightOn = false;
            }

            ToggleLightCommand = ReactiveCommand.Create(ToggleGeneralLight1);
        }

        private void ToggleGeneralLight1()
        {
            try
            {
                if (!IsLightOn)
                {
                    // Turn on the light (intensity logic removed)
                    try
                    {
                        DevicePort.SerialPortInterface.Write("LITA" + 10); // Default to 10 when turning on
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
                    IsLightOn = true;
                }
                else
                {
                    // Turn off the light
                    try
                    {
                        DevicePort.SerialPortInterface.Write("LITA" + 0);
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
                    IsLightOn = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error in General lights 1 ToggleLight: {ex.Message}");
            }
        }
    }

}