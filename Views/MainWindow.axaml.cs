using Avalonia.Controls;
using System;
using ORControlPanelNew.ViewModels;

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
                DataContext = new MainWindowViewModel();
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
