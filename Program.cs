using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;

namespace ORControlPanelNew
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting application...");
                try
                {
                    if (!DevicePort.InitializeDatabase())
                    {
                        Debug.WriteLine("Failed to initialize database. The application will exit.", "Error");
                       
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to initialize database: {ex.Message}\nThe application will exit.{ex} ");
                  
                    return;
                }
                
                
                try
                {
                    if (!DevicePort.SerialPortInterface.Initialize("COM7"))
                    {
                        Debug.WriteLine("Warning: Serial port initialization failed for COM5. Proceeding without serial communication.");
                    }
                }
                catch (Exception ex) {
                    Debug.WriteLine($"Failed to initialize serial port: {ex.Message}");
                }
                var app = BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
                
                Console.WriteLine("Application started successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical error starting application: {ex}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            try
            {
                Console.WriteLine("Building Avalonia app...");
                var builder = AppBuilder.Configure<App>()
                    .UsePlatformDetect()
                    .WithInterFont()
                    .LogToTrace()
                    .UseReactiveUI();
                Console.WriteLine("Avalonia app built successfully");
                return builder;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error building Avalonia app: {ex}");
                throw;
            }
        }
    }
}
