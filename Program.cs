using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using LibVLCSharp.Shared;
using System.IO;

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
                
                // Database initialization
                try
                {
                    Console.WriteLine("Initializing database...");
                    if (!DevicePort.InitializeDatabase())
                    {
                        Console.WriteLine("Failed to initialize database. The application will exit.");
                        return;
                    }
                    Console.WriteLine("Database initialized successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to initialize database: {ex.Message}\nStack trace: {ex.StackTrace}");
                    return;
                }
                
                // Serial port initialization
                try
                {
                    string portName = OperatingSystem.IsMacOS() ? "/dev/tty.usbserial" : "COM7";
                    Console.WriteLine($"Attempting to initialize serial port: {portName}");
                    if (!DevicePort.SerialPortInterface.Initialize(portName))
                    {
                        Console.WriteLine($"Warning: Serial port initialization failed for {portName}. Proceeding without serial communication.");
                    }
                    else
                    {
                        Console.WriteLine("Serial port initialized successfully");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to initialize serial port: {ex.Message}\nStack trace: {ex.StackTrace}");
                }

                // VLC initialization
                //try
                //{
                //    string libVlcPath = OperatingSystem.IsMacOS()
                //        ? "/Applications/VLC.app/Contents/MacOS/lib"
                //        : null;

                //    Console.WriteLine($"Initializing LibVLCSharp with path: {libVlcPath}");
                //    if (OperatingSystem.IsMacOS() && !Directory.Exists(libVlcPath))
                //    {
                //        Console.WriteLine($"Warning: VLC library path does not exist: {libVlcPath}");
                //    }

                //    Core.Initialize(libVlcPath);
                //    Console.WriteLine("LibVLCSharp initialized successfully");
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine($"Failed to initialize LibVLCSharp: {ex.Message}\nStack trace: {ex.StackTrace}");
                //    throw;
                //}

                // Start the application
                Console.WriteLine("Starting Avalonia application...");
                var app = BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
                
                Console.WriteLine("Application started successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical error starting application: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw to see the full error in the console
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
                Console.WriteLine($"Error building Avalonia app: {ex.Message}\nStack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
