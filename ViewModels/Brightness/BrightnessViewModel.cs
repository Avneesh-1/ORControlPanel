using System.Windows.Input;
using ReactiveUI;
using Avalonia.Controls;
using ORControlPanelNew.Views.Brightness;
using System;
using System.Reactive.Linq;
using System.Data;
using System.Diagnostics;

namespace ORControlPanelNew.ViewModels.Brightness
{
    public class BrightnessViewModel : ReactiveObject
    {
        public ICommand OpenDialogCommand { get; }

        private double _generalLight1Intensity;
        public double GeneralLight1Intensity
        {
            get => _generalLight1Intensity;
            set => this.RaiseAndSetIfChanged(ref _generalLight1Intensity, value);
        }

        private double _generalLight2Intensity;
        public double GeneralLight2Intensity
        {
            get => _generalLight2Intensity;
            set => this.RaiseAndSetIfChanged(ref _generalLight2Intensity, value);
        }

        private double _laminarLightIntensity;
        public double LaminarLightIntensity
        {
            get => _laminarLightIntensity;
            set => this.RaiseAndSetIfChanged(ref _laminarLightIntensity, value);
        }

        public BrightnessViewModel()
        {
            // Fetch initial values from the database
            try
            {
                string paramList = "'General Lights 1','General Lights 2','Laminar Light'";
                DataTable dt = DevicePort.ReadValueFromDb(paramList);
                foreach (DataRow row in dt.Rows)
                {
                    string fieldName = row["FieldName"].ToString();
                    string valueStr = row["Value"].ToString();
                    if (double.TryParse(valueStr, out double value))
                    {
                        if (fieldName == "General Lights 1")
                            GeneralLight1Intensity = value;
                        else if (fieldName == "General Lights 2")
                            GeneralLight2Intensity = value;
                        else if (fieldName == "Laminar Light")
                            LaminarLightIntensity = value;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching initial brightness data from DB: {ex.Message}");
                GeneralLight1Intensity = 0;
                GeneralLight2Intensity = 0;
                LaminarLightIntensity = 0;
            }

            // Update logic for each property
            this.WhenAnyValue(x => x.GeneralLight1Intensity)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Subscribe(value =>
                {
                    try
                    {
                        int intValue = (int)value;
                        DevicePort.UpdateValueToDb(intValue.ToString(), "General Lights 1");
                        Debug.WriteLine($"Updated GeneralLight1Intensity: {intValue}");
                        DevicePort.SerialPortInterface.Write("LITA" + intValue);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating GeneralLight1Intensity: {ex.Message}");
                    }
                });

            this.WhenAnyValue(x => x.GeneralLight2Intensity)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Subscribe(value =>
                {
                    try
                    {
                        int intValue = (int)value;
                        DevicePort.UpdateValueToDb(intValue.ToString(), "General Lights 2");
                        Debug.WriteLine($"Updated GeneralLight2Intensity: {intValue}");
                        DevicePort.SerialPortInterface.Write("LITB" + intValue);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating GeneralLight2Intensity: {ex.Message}");
                    }
                });

            this.WhenAnyValue(x => x.LaminarLightIntensity)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Subscribe(value =>
                {
                    try
                    {
                        int intValue = (int)value;
                        DevicePort.UpdateValueToDb(intValue.ToString(), "Laminar Light");
                        Debug.WriteLine($"Updated LaminarLightIntensity: {intValue}");
                        DevicePort.SerialPortInterface.Write("LITI" + intValue);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating LaminarLightIntensity: {ex.Message}");
                    }
                });

            OpenDialogCommand = ReactiveCommand.Create(() =>
            {
                var dialog = new BrightnessDialog();
                dialog.Show();
            });
        }
    }
} 