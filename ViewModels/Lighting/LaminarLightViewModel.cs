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
    public class LaminarLightViewModel : ReactiveObject
    {
        private bool _isLightOn;

        public bool IsLightOn
        {
            get => _isLightOn;
            set => this.RaiseAndSetIfChanged(ref _isLightOn, value);
        }

        public ICommand ToggleLightCommand { get; }
       

        public LaminarLightViewModel()
        {
            try
            {
                string paramList = "'Laminar Light'";
                DataTable dt = DevicePort.ReadValueFromDb(paramList);
                foreach (DataRow row in dt.Rows)
                {
                    string fieldName = row["FieldName"].ToString();
                    string valueStr = row["Value"].ToString();
                    if (double.TryParse(valueStr, out double value))
                    {
                        if (fieldName == "Laminar Light")
                        {
                            IsLightOn = value > 0;
                            Debug.WriteLine($"Fetched LightIntensity from DB: {value}, IsLight1On: {IsLightOn}");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Failed to parse value for {fieldName}: {valueStr}");
                    }
                }
                if (dt.Rows.Cast<DataRow>().All(row => row["FieldName"].ToString() != "Laminar Light"))
                {
                    Debug.WriteLine("No data found for Laminar Lights  in DB, using defaults");
                    IsLightOn = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching initial lighting data from DB: {ex.Message}");
                IsLightOn = false;
            }
            ToggleLightCommand = ReactiveCommand.Create(ToggleLaminarLight);

           
        }

        private void ToggleLaminarLight()
        {
            try
            {
                if (!IsLightOn)
                {
                    // Turn on the light (intensity logic removed)
                    try
                    {
                        DevicePort.SerialPortInterface.Write("LITI"+10); // Default to 10 when turning on
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error writing to serial port: {ex.Message}");
                    }
                    try
                    {
                        DevicePort.UpdateValueToDb("10", "Laminar Light");
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
                        DevicePort.SerialPortInterface.Write("LITI"+0);
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
                Debug.WriteLine($"Unexpected error in Laminar ToggleLight: {ex.Message}");
            }
        }
    }
} 