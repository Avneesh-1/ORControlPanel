using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;

namespace ORControlPanelNew.ViewModels.Ups
{
    public class UpsStatusViewModel : ReactiveObject
    {

        [Reactive] public string UpsStatus { get; set; } = "OFF";
        [Reactive] public bool IsUpsOn { get; set; } = false;

        public UpsStatusViewModel()
        {

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

