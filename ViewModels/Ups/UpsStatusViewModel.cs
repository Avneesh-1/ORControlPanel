using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using ORControlPanelNew.Services;

namespace ORControlPanelNew.ViewModels.Ups
{
    public class UpsStatusViewModel : ReactiveObject
    {
        private readonly Guid _instanceId = Guid.NewGuid();
        private readonly IAlertService _alertService;
        [Reactive] public string UpsStatus { get; set; } = "OFF";
        [Reactive] public bool IsUpsOn { get; set; } = false;

        public UpsStatusViewModel(IAlertService alertService)
        {
            _alertService = alertService;
            DevicePort.DataProcessor.OnUpsStatusUpdated += (isOn) =>
            {
                Log($"Received OnUpsStatusUpdated: isOn={isOn}");
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    UpsStatus = isOn ? "ON" : "OFF";
                    IsUpsOn = isOn;

                });
                if(!isOn)
                {
                    _alertService.ShowAlert("UPS POWER is OFF");
                }
            };

        }
        private static void Log(string message)
        {
            Debug.WriteLine(message);
        }

    }


} 

