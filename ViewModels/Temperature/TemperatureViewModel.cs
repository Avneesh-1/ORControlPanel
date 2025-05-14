using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;

namespace ORControlPanelNew.ViewModels.Temperature
{
    public class TemperatureViewModel : ReactiveObject
    {

        [Reactive] public string Temperature { get; set; } = "0.0";
        [Reactive] public string Humidity { get; set; } = "0.0";
        [Reactive] public string Voltage { get; set; } = "0.0";
        [Reactive] public string Current { get; set; } = "0.0";
        [Reactive] public string TransformerStatus { get; set; } = "OK";
        [Reactive] public string FireStatus { get; set; } = "OFF";
        [Reactive] public string HepaStatus { get; set; } = "BAD";
        [Reactive] public string UpsStatus { get; set; } = "OFF";


        [Reactive] public bool IsUpsOn { get; set; } = false;

        [Reactive] public string AirDiffPressure { get; set; } = "0.0";


        public TemperatureViewModel()
        {
           

            DevicePort.DataProcessor.OnTemperatureUpdated += (temp) =>
            {
                Log($"Received OnTemperatureUpdated: temp={temp}");
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Temperature = temp;
                });
            };

            DevicePort.DataProcessor.OnHumidityUpdated += (humidity) =>
            {
                Log($"Received OnHumidityUpdated: humidity={humidity}");
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Humidity = humidity;
                });
            };

            DevicePort.DataProcessor.OnTransformerUpdated += (voltage, current, isError) =>
            {
                Log($"Received OnTransformerUpdated: voltage={voltage}, current={current}, isError={isError}");
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Voltage = voltage;
                    Current = current;
                    TransformerStatus = isError ? "ERROR" : "OK";
                });
            };

            DevicePort.DataProcessor.onAirDiffPressureUpdated += (adp) =>
            {
                Log($"Received onAirDiffPressureUpdated: ={adp}");
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    AirDiffPressure = adp;
                });
            };

            DevicePort.DataProcessor.OnFireStatusUpdated += (isActive) =>
            {
                Log($"Received OnFireStatusUpdated: isActive={isActive}");
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    FireStatus = isActive ? "ON" : "OFF";
                });
            };

            DevicePort.DataProcessor.OnHepaStatusUpdated += (isBad) =>
            {
                Log($"Received OnHepaStatusUpdated: isBad={isBad}");
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    HepaStatus = isBad ? "BAD" : "GOOD";
                });
            };


            DevicePort.DataProcessor.OnUpsStatusUpdated += (isOn) =>
            {
                Log($"Received OnUpsStatusUpdated: isOn={isOn}");
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    UpsStatus = isOn ? "ON" : "OFF";
                    IsUpsOn = isOn;
                });
            };


        }
        private static void Log(string message)
        {
            Debug.WriteLine(message);
        }

    }


} 

