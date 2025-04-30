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

        public ICommand ToggleLightCommand { get; }
        public ICommand OpenDialogCommand { get; }

        public GeneralLightingViewModel()
        {
            ToggleLightCommand = ReactiveCommand.Create(() =>
            {
                IsLightOn = !IsLightOn;
            });

            OpenDialogCommand = ReactiveCommand.Create(() =>
            {
                var dialog = new LightingDialog();
                dialog.Show();
            });
        }
    }
} 