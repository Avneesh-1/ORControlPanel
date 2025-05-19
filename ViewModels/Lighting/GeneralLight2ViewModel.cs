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
    public class GeneralLight2ViewModel : ReactiveObject
    {
        private bool _isLightOn;

        public bool IsLightOn
        {
            get => _isLightOn;
            set => this.RaiseAndSetIfChanged(ref _isLightOn, value);
        }

        public ICommand ToggleLightCommand { get; }
        public ICommand OpenDialogCommand { get; }

        public GeneralLight2ViewModel()
        {
            try
            {
                // Fetch initial values from database
                string paramList = "'General Lights 2'";
                var dt = DevicePort.ReadValueFromDb(paramList);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    IsLightOn = Convert.ToDouble(row["Value"]) > 0;
                }
                else
                {
                    Debug.WriteLine("No data found for General Lights 2 in DB, using defaults");
                    IsLightOn = false;
                }

                // Optionally sync the hardware with the fetched values
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching initial lighting data from DB: {ex.Message}");
                // Fallback to default values
                IsLightOn = false;
            }

            ToggleLightCommand = ReactiveCommand.Create(() =>
            {
                IsLightOn = !IsLightOn;
            });

            OpenDialogCommand = ReactiveCommand.Create(() =>
            {
                var dialog = new GeneralLight2Dialog();
                dialog.Show();
            });
        }
    }
} 