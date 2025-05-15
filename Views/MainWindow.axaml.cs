using Avalonia.Controls;
using System;
using ORControlPanelNew.ViewModels;
using ORControlPanelNew.Services;

namespace ORControlPanelNew.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            try
            {
                Console.WriteLine("Initializing MainWindow...");
                InitializeComponent();
                this.Opened += (_, _) =>
                {
                    this.WindowState = WindowState.Maximized;

                    this.Topmost = true; // Forces focus on some WMs
                    this.Activate();     // Bring to foreground
                    this.Topmost = false;
                };
                // Create AlertService with this window as the owner
                var alertService = new AlertService(this);

                // Set DataContext with the alert service
                DataContext = new MainWindowViewModel(alertService);
                Console.WriteLine("MainWindow initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing MainWindow: {ex}");
                throw;
            }
        }
    }
}
