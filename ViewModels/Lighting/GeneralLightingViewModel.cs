using System.Windows.Input;
using ReactiveUI;
using System;
using Avalonia.Controls;
using ORControlPanelNew.Views.Lighting;

namespace ORControlPanelNew.ViewModels.Lighting
{
    public class GeneralLightingViewModel : ReactiveObject
    {
        private bool _isLightOn;
        private bool _isDialogOpen;
        private double _light1Intensity;
        private double _light2Intensity;
        private bool _isLight1On;
        private bool _isLight2On;

        public bool IsLightOn
        {
            get => _isLightOn;
            set => this.RaiseAndSetIfChanged(ref _isLightOn, value);
        }

        public bool IsDialogOpen
        {
            get => _isDialogOpen;
            set => this.RaiseAndSetIfChanged(ref _isDialogOpen, value);
        }

        public double Light1Intensity
        {
            get => _light1Intensity;
            set => this.RaiseAndSetIfChanged(ref _light1Intensity, value);
        }

        public double Light2Intensity
        {
            get => _light2Intensity;
            set => this.RaiseAndSetIfChanged(ref _light2Intensity, value);
        }

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

        public ICommand ToggleLightCommand { get; }
        public ICommand OpenDialogCommand { get; }
        public ICommand ToggleLight1Command { get; }
        public ICommand ToggleLight2Command { get; }
        public ICommand CloseDialogCommand { get; }

        public GeneralLightingViewModel()
        {
            Light1Intensity = 0;
            Light2Intensity = 0;
            IsLight1On = false;
            IsLight2On = false;

            ToggleLightCommand = ReactiveCommand.Create(() =>
            {
                IsLightOn = !IsLightOn;
            });

            OpenDialogCommand = ReactiveCommand.Create(() =>
            {
                var dialog = new LightingDialog();
                dialog.Show();
            });

            ToggleLight1Command = ReactiveCommand.Create(() =>
            {
                IsLight1On = !IsLight1On;
            });

            ToggleLight2Command = ReactiveCommand.Create(() =>
            {
                IsLight2On = !IsLight2On;
            });

            CloseDialogCommand = ReactiveCommand.Create(() =>
            {
                IsDialogOpen = false;
            });
        }
    }
} 