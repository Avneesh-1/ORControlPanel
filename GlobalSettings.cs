using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using Microsoft.Data.Sqlite;
using ORControlPanelNew.Models.GasMonitoring;

namespace ORControlPanelNew
{
    internal static class DevicePort
    {
        public static string? CurrentPort { get; set; }


        private static string GetConnectionString()
        {
            // First, try the executable directory
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            string dbPath = Path.Combine(exeDir, "mydata.db");
            string cs = $"Data Source={dbPath};";

            try
            {
                Log($"Attempting to use database in executable directory: {dbPath}");
                using (var connection = new SqliteConnection(cs))
                {
                    connection.Open();
                    Log("Successfully accessed database in executable directory.");
                    return cs;
                }
            }
            catch (Exception ex)
            {
                Log($"Failed to access database in executable directory: {ex.Message}");
                Log("Falling back to user-writable directory (AppData)...");

                // Fall back to AppData directory
                string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appDir = Path.Combine(appDataDir, "ORControlPanel");
                Directory.CreateDirectory(appDir); // Ensure the directory exists
                dbPath = Path.Combine(appDir, "mydata.db");
                cs = $"Data Source={dbPath};";

                Log($"Using fallback database path: {dbPath}");
                return cs;
            }
        }

        public static bool InitializeDatabase()
        {
            try
            {
                string cs = GetConnectionString();
                Log($"Connection string: {cs}");
                if (string.IsNullOrEmpty(cs))
                {
                    Log("Error: Database connection string not found in settings.txt");
                    return false;
                }

                Log("Attempting to connect to database...");
                using (var connection = new SqliteConnection(cs))
                {
                    try
                    {
                    connection.Open();
                        Log("Successfully opened database connection.");
                    }
                    catch (Exception ex)
                    {
                        Log($"Failed to open database connection: {ex.Message}");
                        throw new Exception($"Failed to open database connection: {ex.Message}", ex);
                    }

                    Log("Creating tbl_OT table if it doesn't exist...");
                    using (var command = new SqliteCommand(
                        @"CREATE TABLE IF NOT EXISTS tbl_OT (
                                 FieldName TEXT PRIMARY KEY,
                                 Value TEXT
                             )", connection))
                    {
                        command.ExecuteNonQuery();
                        Log("tbl_OT table created or already exists.");
                    }

                    Log("Creating patientData table if it doesn't exist...");
                    using (var command = new SqliteCommand(
                        @"CREATE TABLE IF NOT EXISTS patientData (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            PatientID TEXT NOT NULL,
                            Name TEXT NOT NULL,
                            Gender TEXT,
                            Age INTEGER,
                            MobileNo TEXT,
                            BloodGroup TEXT,
                            OpDoctor TEXT NOT NULL,
                            AstDoctor TEXT,
                            StartTime TEXT,
                            EndTime TEXT,
                            ot TEXT,
                            created_on TEXT NOT NULL DEFAULT (CURRENT_TIMESTAMP)
                        )", connection))
                    {
                        command.ExecuteNonQuery();
                        Log("patientData table created or already exists.");
                    }

                    Log("Database initialized successfully.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log($"Database initialization failed: {ex.Message}");
                Log($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }


        public static void InsertPatientData(string patientId, string name, string gender, int? age, string mobileNo, string bloodGroup, string opDoctor, string astDoctor, DateTime? startTime, string ot)
        {
            try
            {
                string cs = GetConnectionString();
                Log($"Connection string: {cs}");

                using (var connection = new SqliteConnection(cs))
                {
                    connection.Open();
                    using (var command = new SqliteCommand(
                        "INSERT INTO patientData (PatientID, Name, Gender, Age, MobileNo, BloodGroup, OpDoctor, AstDoctor, StartTime, EndTime, ot) " +
                        "VALUES (@PatientID, @Name, @Gender, @Age, @MobileNo, @BloodGroup, @OpDoctor, @AstDoctor, @StartTime, @EndTime, @ot)",
                        connection))
                    {
                        command.Parameters.AddWithValue("@PatientID", patientId ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Name", name ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Gender", gender ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Age", age.HasValue ? age.Value : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@MobileNo", mobileNo ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@BloodGroup", bloodGroup ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@OpDoctor", opDoctor ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@AstDoctor", astDoctor ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@StartTime", startTime.HasValue ? startTime.Value.ToString("o") : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@EndTime", (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ot", ot ?? (object)DBNull.Value);
                        int rowsAffected = command.ExecuteNonQuery();
                        Log($"Inserted {rowsAffected} row(s) into patientData: PatientID={patientId}");
                    }
                }
            }
            catch (Exception ex)
                {
                Log($"Failed to insert patient data: {ex.Message}, InnerException: {ex.InnerException?.Message}");
                throw new Exception($"Failed to insert patient data: {ex.Message}", ex);
            }
                }

        public static void UpdateValueToDb(string value, string fieldName)
        {
            try
            {
                string cs = GetConnectionString();
                Log($"Connection string: {cs}");
                if (string.IsNullOrEmpty(cs))
                {
                    Log("Error: Database connection string not found in settings.txt");
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
                        Log($"UPDATE affected {rowsAffected} rows for FieldName={fieldName}, Value={value}");
                        if (rowsAffected == 0)
                        {
                            Log($"No rows found for FieldName={fieldName}. Inserting new row.");
                            using (var insertCommand = new SqliteCommand(
                                "INSERT INTO tbl_OT (FieldName, Value) VALUES (@FieldName, @Value)", connection))
                            {
                                insertCommand.Parameters.AddWithValue("@FieldName", fieldName);
                                insertCommand.Parameters.AddWithValue("@Value", value);
                                insertCommand.ExecuteNonQuery();
                                Log($"Inserted new row: FieldName={fieldName}, Value={value}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Database update failed: {ex.Message}, InnerException: {ex.InnerException?.Message}");
                throw new Exception($"Database update failed: {ex.Message}", ex);
            }
        }

        public static DataTable ReadValueFromDb(string paramList)
        {
            try
            {
                string cs = GetConnectionString();
                Log($"Connection string: {cs}");

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
                Log($"Database read failed: {ex.Message}, InnerException: {ex.InnerException?.Message}");
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

        private static void Log(string message)
        {
            Debug.WriteLine(message);
        }

        internal class SerialPortInterface
        {
            private static SerialPort _myCOMPort = new SerialPort();
            public static event Action<string> OnDataReceived;

            public static bool Initialize(string portName, int baudRate = 9600)
            {
                try
                {
                  

                    _myCOMPort.PortName = portName;
                    _myCOMPort.BaudRate = baudRate;
                    _myCOMPort.Open();
                    
                    _myCOMPort.DataReceived += (s, e) =>
                    {
                        try
                        {
                            string data = _myCOMPort.ReadLine();
                            Log($"Serial data received: {data}");
                            DataProcessor.ProcessData(data);
                            OnDataReceived?.Invoke(data);
                        }
                        catch (Exception ex)
                        {
                            Log($"Serial read failed: {ex.Message}");
                        }
                    };

                    _myCOMPort.Open();
                    Log($"Serial port {portName} opened.");

                    return true;
                }
                catch (Exception ex)
                {
                    Log($"Serial port initialization failed: {ex.Message}, InnerException: {ex.InnerException?.Message}");
                    return false;
                }
            }

            public static void Write(string data)
            {
                try
                {
                    

                    if (!_myCOMPort.IsOpen)
                    {
                        Log($"Opening port {_myCOMPort.PortName}...");
                        _myCOMPort.Open();
                    }

                    _myCOMPort.Write(data );
                    Log($"Data written to port: {data} AT {_myCOMPort.PortName}");
                }
                catch (Exception ex)
                {
                    Log($"Serial port write failed: {ex.Message}, InnerException: {ex.InnerException?.Message}");
                }
            }

            public static void Close()
            {
                try
                {
                    if (_myCOMPort != null && _myCOMPort.IsOpen)
                    {
                        _myCOMPort.Close();
                        Log("Serial port closed.");
                    }
                }
                catch (Exception ex)
                {
                    Log($"Serial port close failed: {ex.Message}");
                }
            }
        }

        internal class SystemInfo
        {
            public static string Oxygen { get; set; } = "0";
            public static string Nitrogen { get; set; } = "0";
            public static string CO2 { get; set; } = "0";
            public static string Air7 { get; set; } = "0";
            public static string Air4 { get; set; } = "0";
            public static string Vacuum { get; set; } = "0";
            public static string DiffPress { get; set; } = "0";
            public static string Temperature { get; set; } = "0";
            public static string TemperatureSetValue { get; set; } = "0";
            public static string Humidity { get; set; } = "0";
            public static string Voltage { get; set; } = "0";
            public static string Current { get; set; } = "0";
        }

        internal class DataProcessor
        {
            public static event Action<string, string> OnGasPressureUpdated; // (GasName, Pressure)
            public static event Action<string, bool> OnGasAlertUpdated; // (GasName, IsAlert)
            public static event Action<bool> OnGeneralGasAlertUpdated;
            public static event Action<bool> OnCallReceivedUpdated;
            public static event Action<string> OnTemperatureUpdated;
            public static event Action<string> OnHumidityUpdated;
            public static event Action<string, string, bool> OnTransformerUpdated;
            public static event Action<bool> OnFireStatusUpdated;
            public static event Action<bool> OnHepaStatusUpdated;
            public static event Action<bool> OnUpsStatusUpdated;
            public static event Action<string> onAirDiffPressureUpdated;

            public static void ProcessData(string inData)
            {
                string[] allData = inData.Split('#');
                Debug.WriteLine(allData,"DATA TRRRRR");
                foreach (string s in allData)
                {
                    if (string.IsNullOrWhiteSpace(s))
                        continue;

                    Log($"{DateTime.Now}: Processing data: {s}");

                    if (s.StartsWith("CALN"))
                    {
                        string[] parts = s.Split('$');
                        if (parts.Length > 1)
                        {
                            bool isActive = parts[1] == "1";
                            Log($"CALN: Invoking OnCallReceivedUpdated with isActive={isActive}");
                            OnCallReceivedUpdated?.Invoke(isActive);
                        }
                    }

                    if (s.StartsWith("GASR"))
                    {
                        string[] parts = s.Split('$');
                        if (parts.Length > 1)
                        {
                            bool isAlert = parts[1] == "1";
                            if (isAlert)
                            {
                                Log($"GASR: Invoking OnGeneralGasAlertUpdated with isAlert={isAlert}");
                                OnGeneralGasAlertUpdated?.Invoke(true);
                            }
                        }
                    }

                    if (s.StartsWith("GASW"))
                    {
                        string[] parts = s.Split('$');
                        if (parts.Length > 1)
                        {
                            bool isAlert = parts[1] == "0";
                            if (isAlert)
                            {
                                Log($"GASW: Invoking OnGeneralGasAlertUpdated with isAlert={isAlert}");
                                OnGeneralGasAlertUpdated?.Invoke(false);
                            }
                        }
                    }

                    // Gas Alert Handling
                    if (s.StartsWith("ALGA"))
                    {
                        Log($"ALGA: Invoking OnGasAlertUpdated for O₂ with isAlert=true");
                        OnGasAlertUpdated?.Invoke("O₂", true);
                    }
                    if (s.StartsWith("BLGA"))
                    {
                        Log($"BLGA: Invoking OnGasAlertUpdated for O₂ with isAlert=false");
                        OnGasAlertUpdated?.Invoke("O₂", false);
                    }

                    if (s.StartsWith("ALGB"))
                    {
                        Log($"ALGB: Invoking OnGasAlertUpdated for N₂O with isAlert=true");
                        OnGasAlertUpdated?.Invoke("N₂O", true);
                    }
                    if (s.StartsWith("BLGB"))
                    {
                        Log($"BLGB: Invoking OnGasAlertUpdated for N₂O with isAlert=false");
                        OnGasAlertUpdated?.Invoke("N₂O", false);
                    }

                    if (s.StartsWith("ALGC"))
                    {
                        Log($"ALGC: Invoking OnGasAlertUpdated for CO₂ with isAlert=true");
                        OnGasAlertUpdated?.Invoke("CO₂", true);
                    }
                    if (s.StartsWith("BLGC"))
                    {
                        Log($"BLGC: Invoking OnGasAlertUpdated for CO₂ with isAlert=false");
                        OnGasAlertUpdated?.Invoke("CO₂", false);
                    }

                    if (s.StartsWith("ALGD"))
                    {
                        Log($"ALGD: Invoking OnGasAlertUpdated for AIR 4 with isAlert=true");
                        OnGasAlertUpdated?.Invoke("AIR 4", true);
                    }
                    if (s.StartsWith("BLGD"))
                    {
                        Log($"BLGD: Invoking OnGasAlertUpdated for AIR 4 with isAlert=false");
                        OnGasAlertUpdated?.Invoke("AIR 4", false);
                    }

                    if (s.StartsWith("ALGE"))
                    {
                        Log($"ALGE: Invoking OnGasAlertUpdated for AIR 7 with isAlert=true");
                        OnGasAlertUpdated?.Invoke("AIR 7", true);
                    }
                    if (s.StartsWith("BLGE"))
                    {
                        Log($"BLGE: Invoking OnGasAlertUpdated for AIR 7 with isAlert=false");
                        OnGasAlertUpdated?.Invoke("AIR 7", false);
                    }

                    if (s.StartsWith("ALGF"))
                    {
                        Log($"ALGF: Invoking OnGasAlertUpdated for VAC with isAlert=true");
                        OnGasAlertUpdated?.Invoke("VAC", true);
                    }
                    if (s.StartsWith("BLGF"))
                    {
                        Log($"BLGF: Invoking OnGasAlertUpdated for VAC with isAlert=false");
                        OnGasAlertUpdated?.Invoke("VAC", false);
                    }

                    // Gas Pressure Updates
                    if (s.StartsWith("RDGA"))
                    {
                        string[] parts = s.Split('$');
                        if (parts.Length > 1)
                        {
                            string pressureStr = parts[1];
                            SystemInfo.Oxygen = pressureStr;
                            Log($"RDGA: Invoking OnGasPressureUpdated for O₂ with pressure={pressureStr}");
                            OnGasPressureUpdated?.Invoke("O₂", pressureStr);
                        }
                        else
                        {
                            Log($"RDGA: Failed to parse pressure from {s}");
                        }
                    }
                    if (s.StartsWith("RDGB"))
                    {
                        string[] parts = s.Split('$');
                        if (parts.Length > 1)
                        {
                            string pressureStr = parts[1];
                            SystemInfo.Nitrogen = pressureStr;
                            Log($"RDGB: Invoking OnGasPressureUpdated for N₂O with pressure={pressureStr}");
                            OnGasPressureUpdated?.Invoke("N₂O", pressureStr);
                        }
                        else
                        {
                            Log($"RDGB: Failed to parse pressure from {s}");
                        }
                    }
                    if (s.StartsWith("RDGC"))
                    {
                        string[] parts = s.Split('$');
                        if (parts.Length > 1)
                        {
                            string pressureStr = parts[1];
                            SystemInfo.CO2 = pressureStr;
                            Log($"RDGC: Invoking OnGasPressureUpdated for CO₂ with pressure={pressureStr}");
                            OnGasPressureUpdated?.Invoke("CO₂", pressureStr);
                        }
                        else
                        {
                            Log($"RDGC: Failed to parse pressure from {s}");
                        }
                    }
                    if (s.StartsWith("RDGD"))
                    {
                        string[] parts = s.Split('$');
                        if (parts.Length > 1)
                        {
                            string pressureStr = parts[1];
                            SystemInfo.Air4 = pressureStr;
                            Log($"RDGD: Invoking OnGasPressureUpdated for AIR 4 with pressure={pressureStr}");
                            OnGasPressureUpdated?.Invoke("AIR 4", pressureStr);
                        }
                        else
                        {
                            Log($"RDGD: Failed to parse pressure from {s}");
                        }
                    }
                    if (s.StartsWith("RDGE"))
                    {
                        string[] parts = s.Split('$');
                        if (parts.Length > 1)
                        {
                            string pressureStr = parts[1];
                            SystemInfo.Air7 = pressureStr;
                            Log($"RDGE: Invoking OnGasPressureUpdated for AIR 7 with pressure={pressureStr}");
                            OnGasPressureUpdated?.Invoke("AIR 7", pressureStr);
                        }
                        else
                        {
                            Log($"RDGE: Failed to parse pressure from {s}");
                        }
                    }
                    if (s.StartsWith("RDGF"))
                    {
                        string[] parts = s.Split('$');
                        if (parts.Length > 1)
                        {
                            string pressureStr = parts[1];
                            SystemInfo.Vacuum = pressureStr;
                            Log($"RDGF: Invoking OnGasPressureUpdated for VAC with pressure={pressureStr}");
                            OnGasPressureUpdated?.Invoke("VAC", pressureStr);
                        }
                        else
                        {
                            Log($"RDGF: Failed to parse pressure from {s}");
                        }
                    }

                    if (s.StartsWith("ARDP"))
                    {
                        string[] parts = s.Split('$');
                        if (parts.Length > 1)
                        {
                            string diffPressure = parts[1];
                            SystemInfo.DiffPress = parts[1];
                            Log($"ARDP: Updated DiffPress to {parts[1]}");
                            onAirDiffPressureUpdated?.Invoke( diffPressure);
                        }
                    }

                    if (s.StartsWith("TEMP"))
                    {
                        string[] parts = s.Split('$');
                        if (parts.Length > 1)
                        {
                            string temp = parts[1];
                            SystemInfo.Temperature = temp;
                            Log($"TEMP: Invoking OnTemperatureUpdated with temp={temp}");
                            OnTemperatureUpdated?.Invoke(temp);
                        }
                    }

                    if (s.StartsWith("HUMD"))
                    {
                        string[] parts = s.Split('$');
                        if (parts.Length > 1)
                        {
                            string humidity = parts[1];
                            SystemInfo.Humidity = humidity;
                            Log($"HUMD: Invoking OnHumidityUpdated with humidity={humidity}");
                            OnHumidityUpdated?.Invoke(humidity);
                        }
                    }

                    if (s.StartsWith("ITID"))
                    {
                        string[] ts = s.Substring(4).Split('$');
                        string voltage = "0";
                        string current = "0";
                        bool isError = false;
                        foreach (var s1 in ts)
                        {
                            if (s1.StartsWith("V"))
                            {
                                voltage = s1.Substring(1);
                                SystemInfo.Voltage = voltage;
                            }
                            if (s1.StartsWith("C"))
                            {
                                current = s1.Substring(1);
                                SystemInfo.Current = current;
                            }
                            if (s1.StartsWith("ST"))
                            {
                                isError = s1.Substring(2).Contains("1");
                            }
                        }
                        Log($"ITID: Invoking OnTransformerUpdated with voltage={voltage}, current={current}, isError={isError}");
                        OnTransformerUpdated?.Invoke(voltage, current, isError);
                    }

                    if (s.StartsWith("FRAM"))
                    {
                        string[] parts = s.Split('$');
                        if (parts.Length > 1 && float.TryParse(parts[1], out float val))
                        {
                            bool isActive = val >= 10;
                            Log($"FRAM: Invoking OnFireStatusUpdated with isActive={isActive}");
                            OnFireStatusUpdated?.Invoke(isActive);
                        }
                        else
                        {
                            Log($"FRAM: Invoking OnFireStatusUpdated with default isActive=false");
                            OnFireStatusUpdated?.Invoke(false);
                        }
                    }

                    if (s.StartsWith("HFST"))
                    {
                        string[] parts = s.Split('$');
                        if (parts.Length > 1 && float.TryParse(parts[1], out float val))
                        {
                            bool isBad = val >= 10;
                            Log($"HFST: Invoking OnHepaStatusUpdated with isBad={isBad}");
                            OnHepaStatusUpdated?.Invoke(isBad);
                        }
                        else
                        {
                            Log($"HFST: Invoking OnHepaStatusUpdated with default isBad=false");
                            OnHepaStatusUpdated?.Invoke(false);
                        }
                    }

                    if (s.StartsWith("UPSS"))
                    {
                        string[] parts = s.Split('$');
                        if (parts.Length > 1)
                        {
                            bool isOn = parts[1] == "1";
                            Log($"UPSS: Invoking OnUpsStatusUpdated with isOn={isOn}");
                            OnUpsStatusUpdated?.Invoke(isOn);
                        }
                    }
                }
            }
        }
    }
}