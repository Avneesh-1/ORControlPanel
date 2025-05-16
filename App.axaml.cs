using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ORControlPanelNew.ViewModels;
using ORControlPanelNew.Views;
using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace ORControlPanelNew
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            try
            {
                Console.WriteLine("Initializing application...");
                AvaloniaXamlLoader.Load(this);
                Console.WriteLine("XAML loaded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Initialize: {ex}");
            }
        }

        public override async void OnFrameworkInitializationCompleted()
        {
            try
            {
                Console.WriteLine("Framework initialization completed...");
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    // Show splash screen first
                    var splash = new SplashScreen();
                    desktop.MainWindow = splash;
                    splash.Show();

                    // Wait 5 seconds
                    await Task.Delay(5000);

                    // After splash, show main window
                    var mainWindow = new MainWindow();
                    desktop.MainWindow = mainWindow;
                    mainWindow.Show();
                    splash.Close();
                }
                else
                {
                    Console.WriteLine("ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime");
                }

                base.OnFrameworkInitializationCompleted();
                Console.WriteLine("Base OnFrameworkInitializationCompleted called");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnFrameworkInitializationCompleted: {ex}");
            }
        }
    }
}
