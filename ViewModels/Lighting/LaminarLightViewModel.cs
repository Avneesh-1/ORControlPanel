using System.Windows.Input;
using ReactiveUI;
using System;
using ORControlPanelNew.Views.Lighting;

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
            ToggleLightCommand = ReactiveCommand.Create(() =>
            {
                IsLightOn = !IsLightOn;
            });

            OpenDialogCommand = ReactiveCommand.Create(() =>
            {
                var dialog = new LaminarLightDialog();
                dialog.Show();
            });
        }
    }
} 