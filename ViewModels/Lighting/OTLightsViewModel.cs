using System.Windows.Input;
using ReactiveUI;
using System;
using ORControlPanelNew.Views.Lighting;

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
            ToggleLight1Command = ReactiveCommand.Create(() =>
            {
                IsLight1On = !IsLight1On;
            });

            ToggleLight2Command = ReactiveCommand.Create(() =>
            {
                IsLight2On = !IsLight2On;
            });

            OpenDialogCommand = ReactiveCommand.Create(() =>
            {
                var dialog = new OTLightsDialog();
                dialog.Show();
            });
        }
    }
} 