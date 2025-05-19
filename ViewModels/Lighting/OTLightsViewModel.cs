using System.Windows.Input;
using ReactiveUI;
using System;
using ORControlPanelNew.Views.Lighting;
using System.Diagnostics;
using System.Data;
using System.Linq;

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
            try
            {
                // Fetch data for "OTLight1" and "OTLight2"
                string paramList = "'OTLight1','OTLight2'";
                DataTable dt = DevicePort.ReadValueFromDb(paramList);

                // Process the DataTable to set initial values
                foreach (DataRow row in dt.Rows)
                {
                    string fieldName = row["FieldName"].ToString();
                    string valueStr = row["Value"].ToString();

                    if (fieldName == "OTLight1")
                    {
                        IsLight1On = valueStr == "1"; // Light is on if value is "1"
                        Debug.WriteLine($"Fetched OTLight1 state from DB: IsLight1On: {IsLight1On}");
                    }
                    else if (fieldName == "OTLight2")
                    {
                        IsLight2On = valueStr == "1"; // Light is on if value is "1"
                        Debug.WriteLine($"Fetched OTLight2 state from DB: IsLight2On: {IsLight2On}");
                    }
                }

                // If no data was found for a light, use default values
                if (dt.Rows.Cast<DataRow>().All(row => row["FieldName"].ToString() != "OTLight1"))
                {
                    Debug.WriteLine("No data found for OTLight1 in DB, using default (off)");
                    IsLight1On = false;
                }
                if (dt.Rows.Cast<DataRow>().All(row => row["FieldName"].ToString() != "OTLight2"))
                {
                    Debug.WriteLine("No data found for OTLight2 in DB, using default (off)");
                    IsLight2On = false;
                }

                // Sync the hardware with the fetched values
                try
                {
                    DevicePort.SerialPortInterface.Write("LITE" + (IsLight1On ? "1" : "0"));
                    DevicePort.SerialPortInterface.Write("LITF" + (IsLight2On ? "1" : "0"));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error syncing OT Lights with hardware: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching initial OT Lights data from DB: {ex.Message}");
                // Fallback to default values
                IsLight1On = false;
                IsLight2On = false;
            }

            ToggleLight1Command = ReactiveCommand.Create(ToggleOTLight1);

            ToggleLight2Command = ReactiveCommand.Create(ToggleOTLight2);

           
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
                        DevicePort.SerialPortInterface.Write("LITE1");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error writing to serial port for OTLight1: {ex.Message}");
                    }

                    // Attempt to update database
                    try
                    {
                        DevicePort.UpdateValueToDb("1", "OTLight1");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating database for OTLight1: {ex.Message}");
                    }

                    
                    IsLight1On = true;
                }
                else
                {
                    // Turn off the light
                    try
                    {
                        DevicePort.SerialPortInterface.Write("LITE0");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error writing to serial port for OTLight1: {ex.Message}");
                    }

                    // Attempt to update database
                    try
                    {
                        DevicePort.UpdateValueToDb("0", "OTLight1");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating database for OTLight1: {ex.Message}");
                    }

                    IsLight1On = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error in ToggleOTLight1: {ex.Message}");
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
                        DevicePort.SerialPortInterface.Write("LITF1");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error writing to serial port for OTLight2: {ex.Message}");
                    }

                    // Attempt to update database
                    try
                    {
                        DevicePort.UpdateValueToDb("1", "OTLight2");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating database for OTLight2: {ex.Message}");
                    }

                   
                    IsLight2On = true;
                }
                else
                {
                    // Turn off the light
                    try
                    {
                        DevicePort.SerialPortInterface.Write("LITF0");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error writing to serial port for OTLight2: {ex.Message}");
                    }

                    // Attempt to update database
                    try
                    {
                        DevicePort.UpdateValueToDb("0", "OTLight2");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating database for OTLight2: {ex.Message}");
                    }

                    IsLight2On = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error in ToggleOTLight2: {ex.Message}");
            }
        }
    }
} 