using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace ORControlPanelNew
{
    internal static class DevicePort
    {
        public static string? CurrentPort { get; set; }

        public static bool InitializeDatabase()
        {
            try
            {
                string settingsPath = "settings.txt";
                if (!File.Exists(settingsPath))
                {
                    Console.WriteLine($"Error: {settingsPath} not found.");
                    return false;
                }

                var cs = File.ReadLines(settingsPath).FirstOrDefault();
                if (string.IsNullOrEmpty(cs))
                {
                    Console.WriteLine("Error: Database connection string not found in settings.txt");
                    return false;
                }

                using (var connection = new SqliteConnection(cs))
                {
                    connection.Open();
                    using (var command = new SqliteCommand(
                        @"CREATE TABLE IF NOT EXISTS tbl_OT (
                    FieldName TEXT PRIMARY KEY,
                    Value TEXT
                )", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    Console.WriteLine("Database initialized successfully.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database initialization failed: {ex.Message}");
                return false;
            }
        }

        public static void UpdateValueToDb(string value, string fieldName)
        {
            try
            {
                string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.txt");
                if (!File.Exists(settingsPath))
                {
                    Console.WriteLine($"Error: {settingsPath} not found.");
                    return;
                }

                var cs = File.ReadLines(settingsPath).FirstOrDefault();
                Console.WriteLine($"Connection string: {cs}");
                if (string.IsNullOrEmpty(cs))
                {
                    Console.WriteLine("Error: Database connection string not found in settings.txt");
                    return;
                }

                using (var connection = new SqliteConnection(cs))
                {
                    connection.Open();
                    using (var command = new SqliteCommand("UPDATE tbl_OT SET Value = @Value WHERE FieldName = @FieldName", connection))
                    {
                        command.Parameters.AddWithValue("@Value", value);
                        command.Parameters.AddWithValue("@FieldName", fieldName);
                        int rowsAffected = command.ExecuteNonQuery();
                        Console.WriteLine($"UPDATE affected {rowsAffected} rows for FieldName={fieldName}, Value={value}");
                        if (rowsAffected == 0)
                        {
                            Console.WriteLine($"No rows found for FieldName={fieldName}. Inserting new row.");
                            using (var insertCommand = new SqliteCommand(
                                "INSERT INTO tbl_OT (FieldName, Value) VALUES (@FieldName, @Value)", connection))
                            {
                                insertCommand.Parameters.AddWithValue("@FieldName", fieldName);
                                insertCommand.Parameters.AddWithValue("@Value", value);
                                insertCommand.ExecuteNonQuery();
                                Console.WriteLine($"Inserted new row: FieldName={fieldName}, Value={value}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database update failed: {ex.Message}, InnerException: {ex.InnerException?.Message}");
                throw new Exception($"Database update failed: {ex.Message}", ex); // Keep for debugging
            }
        }

        public static DataTable ReadValueFromDb(string paramList)
        {
            try
            {
                var cs = File.ReadLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.txt")).FirstOrDefault();
                if (string.IsNullOrEmpty(cs))
                    throw new InvalidOperationException("Database connection string not found in settings.txt");

                var fieldNames = paramList.Split(',').Select(x => x.Trim(' ', '\'')).ToArray();
                var paramPlaceholders = string.Join(",", fieldNames.Select((_, i) => $"@p{i}"));

                using (var connection = new SqliteConnection(cs))
                {
                    connection.Open();
                    using (var command = new SqliteCommand($"SELECT * FROM tbl_OT WHERE FieldName IN ({paramPlaceholders})", connection))
                    {
                        for (int i = 0; i < fieldNames.Length; i++)
                        {
                            command.Parameters.AddWithValue($"@p{i}", fieldNames[i]);
                        }

                        using (var reader = command.ExecuteReader())
                        {
                            var dt = new DataTable();
                            dt.Load(reader);
                            return dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Database read failed: {ex.Message}", ex);
            }
        }

        public static void OpenKeyboard()
        {
            try
            {
                var processes = Process.GetProcessesByName("osk");
                if (processes.Length == 0)
                {
                    Process.Start("osk.exe");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to open keyboard: {ex.Message}", ex);
            }
        }

        internal class SerialPortInterface
        {
            private static SerialPort _myCOMPort = new SerialPort();

            public static void Initialize(string portName, int baudRate = 9600)
            {
                try
                {
                    _myCOMPort.PortName = portName;
                    _myCOMPort.BaudRate = baudRate;
                    DevicePort.CurrentPort = portName;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Serial port initialization failed: {ex.Message}", ex);
                }
            }

            public static void Write(string data)
            {
                try
                {
                    var availablePorts = SerialPort.GetPortNames();
                    Debug.WriteLine("Available ports: " + string.Join(", ", availablePorts));

                    if (_myCOMPort == null)
                        throw new Exception("Serial port is not initialized.");

                    if (!_myCOMPort.IsOpen)
                    {
                        _myCOMPort.Open();
                        Debug.WriteLine("Port opened successfully.");
                    }

                    _myCOMPort.Write(data + Environment.NewLine);
                    Debug.WriteLine($"Data written to port: {data}");
                }
                catch (FileNotFoundException fnfEx)
                {
                    Debug.WriteLine($"FileNotFoundException: {fnfEx.Message}");
                    throw new Exception("Serial port file not found", fnfEx);
                }
                catch (UnauthorizedAccessException uaEx)
                {
                    Debug.WriteLine($"UnauthorizedAccessException: {uaEx.Message}");
                    throw new Exception("Access to the serial port is denied", uaEx);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception: {ex.Message}");
                    throw new Exception($"Serial port write failed: {ex.Message}", ex);
                }
            }


            public static void Close()
            {
                try
                {
                    if (_myCOMPort.IsOpen)
                        _myCOMPort.Close();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Serial port close failed: {ex.Message}", ex);
                }
            }
        }

        internal class GasPressure
        {
            public static string Oxygen { get; set; } = "0";
            public static string Nitrogen { get; set; } = "0";
            public static string CO2 { get; set; } = "0";
            public static string Air7 { get; set; } = "0";
            public static string Air4 { get; set; } = "0";
            public static string Vacuum { get; set; } = "0";
            public static string DiffPress { get; set; } = "0";
        }
    }
}
