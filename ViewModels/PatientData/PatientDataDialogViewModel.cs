using System;
using System.Windows.Input;
using ReactiveUI;
using Avalonia.Controls;

namespace ORControlPanelNew.ViewModels.PatientData
{
    public class PatientDataDialogViewModel : ReactiveObject
    {
        private string _patientName;
        private string _age;
        private string _procedure;
        private string _surgeonName;
        private string _errorMessage;

        public string PatientName
        {
            get => _patientName;
            set => this.RaiseAndSetIfChanged(ref _patientName, value);
        }

        public string Age
        {
            get => _age;
            set => this.RaiseAndSetIfChanged(ref _age, value);
        }

        public string Procedure
        {
            get => _procedure;
            set => this.RaiseAndSetIfChanged(ref _procedure, value);
        }

        public string SurgeonName
        {
            get => _surgeonName;
            set => this.RaiseAndSetIfChanged(ref _surgeonName, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public ICommand SubmitCommand { get; }
        public ICommand CloseCommand { get; }

        public PatientDataDialogViewModel(Window dialog)
        {
            SubmitCommand = ReactiveCommand.Create(() =>
            {
                try
                {
                    // Validate required fields
                    if (string.IsNullOrWhiteSpace(PatientName))
                    {
                        ErrorMessage = "Patient Name is required.";
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(Procedure))
                    {
                        ErrorMessage = "Procedure is required.";
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(SurgeonName))
                    {
                        ErrorMessage = "Surgeon Name is required.";
                        return;
                    }

                    // Validate Age (optional, but must be a valid integer if provided)
                    int? ageValue = null;
                    if (!string.IsNullOrWhiteSpace(Age))
                    {
                        if (!int.TryParse(Age, out int parsedAge) || parsedAge < 0)
                        {
                            ErrorMessage = "Age must be a valid positive number.";
                            return;
                        }
                        ageValue = parsedAge;
                    }

                    // Insert into database
                    DevicePort.InsertPatientData(
                        patientId: Guid.NewGuid().ToString(), // Generate a unique PatientID
                        name: PatientName,
                        gender: null, // Not captured in the form
                        age: ageValue,
                        mobileNo: null, // Not captured in the form
                        bloodGroup: null, // Not captured in the form
                        opDoctor: SurgeonName,
                        astDoctor: null, // Not captured in the form
                        startTime: DateTime.Now, // Current time
                        ot: Procedure
                    );

                    // Create the message box
                    var messageBox = new Window
                    {
                        Title = "Success",
                        Width = 300,
                        Height = 150,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };

                    // Create the OK command to close the message box
                    var okCommand = ReactiveCommand.Create(() => messageBox.Close());

                    // Set the content of the message box
                    messageBox.Content = new StackPanel
                    {
                        Spacing = 10,
                        Children =
                        {
                            new TextBlock { Text = "Patient data saved successfully!", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center },
                            new Button { Content = "OK", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Command = okCommand }
                        }
                    };

                    // Show the message box as a dialog
                    messageBox.ShowDialog(dialog);

                    // Clear the form
                    PatientName = string.Empty;
                    Age = string.Empty;
                    Procedure = string.Empty;
                    SurgeonName = string.Empty;
                    ErrorMessage = string.Empty;

                    // Close the dialog
                    dialog.Close();
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Failed to save patient data: {ex.Message}";
                }
            });

            CloseCommand = ReactiveCommand.Create(() =>
            {
                dialog.Close();
            });
        }
    }
}