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
            try
            {
              
                string paramList = "'Laminar Light'";
                DataTable dt = DevicePort.ReadValueFromDb(paramList);

                // Process the DataTable to set initial values
                foreach (DataRow row in dt.Rows)
                {
                    string fieldName = row["FieldName"].ToString();
                    string valueStr = row["Value"].ToString();

                    if (double.TryParse(valueStr, out double value))
                    {
                        if (fieldName == "Laminar Light")
                        {
                            LightIntensity = value;
                            IsLightOn = value > 0; // Light is on if intensity > 0
                            Debug.WriteLine($"Fetched LightIntensity from DB: {LightIntensity}, IsLight1On: {IsLightOn}");
                        }
                     
                    }
                    else
                    {
                        Debug.WriteLine($"Failed to parse value for {fieldName}: {valueStr}");
                    }
                }

                // If no data was found for a light, use default values
                if (dt.Rows.Cast<DataRow>().All(row => row["FieldName"].ToString() != "Laminar Light"))
                {
                    Debug.WriteLine("No data found for Laminar Lights  in DB, using defaults");
                    LightIntensity = 0;
                    IsLightOn = false;
                }
              

                // Optionally sync the hardware with the fetched values
                if (IsLightOn && LightIntensity > 0)
        {
                    DevicePort.SerialPortInterface.Write("LITI" + (int)LightIntensity);
                }
              
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching initial lighting data from DB: {ex.Message}");
                // Fallback to default values
                LightIntensity = 0;
               
                IsLightOn = false;
            
            }
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