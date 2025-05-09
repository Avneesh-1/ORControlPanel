using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ORControlPanelNew.Views;
using System;

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

        public override void OnFrameworkInitializationCompleted()
        {
            try
            {
                Console.WriteLine("Framework initialization completed...");
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    Console.WriteLine("Creating main window...");
                    desktop.MainWindow = new MainWindow();
                    Console.WriteLine("Main window created and set");
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