using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using Microsoft.Data.Sqlite;
using ORControlPanelNew.Models.GasMonitoring;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ORControlPanelNew
{
    internal static class DevicePort
    {
        public static string? CurrentPort { get; set; }
        private static readonly object _dbLock = new object();

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
                lock (_dbLock)
                {
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

                        Log("Creating phonebook table if it doesn't exist...");
                        using (var command = new SqliteCommand(
                            @"CREATE TABLE IF NOT EXISTS phonebook (
                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                Name TEXT NOT NULL,
                                Number TEXT NOT NULL
                            )", connection))
                        {
                            command.ExecuteNonQuery();
                            Log("phonebook table created or already exists.");
                        }
    
                        Log("Database initialized successfully.");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Database initialization failed: {ex.Message}");
                Log($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }


        public static List<ContactData> GetContacts()
        {
            var contacts = new List<ContactData>();
            try
            {
                string cs = GetConnectionString();
                using (var connection = new SqliteConnection(cs))
                {
                    connection.Open();
                    using (var command = new SqliteCommand("SELECT Id, Name, Number FROM phonebook", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                contacts.Add(new ContactData
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Number = reader.GetString(2)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Failed to get contacts: {ex.Message}");
            }
            return contacts;
        }

        public static void AddContact(string name, string number)
        {
            try
            {
                string cs = GetConnectionString();
                using (var connection = new SqliteConnection(cs))
                {
                    connection.Open();
                    using (var command = new SqliteCommand("INSERT INTO phonebook (Name, Number) VALUES (@Name, @Number)", connection))
                    {
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Number", number);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Failed to add contact: {ex.Message}");
            }
        }

        public static void DeleteFromPhonebook(int id)
        {
            try
            {
                string cs = GetConnectionString();
                using (var connection = new SqliteConnection(cs))
                {
                    connection.Open();
                    using (var command = new SqliteCommand("DELETE FROM phonebook WHERE Id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Failed to delete contact: {ex.Message}");
            }
        }

        public class ContactData
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Number { get; set; } = string.Empty;
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
                    lock (_dbLock)
                    {
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
            }
            catch (Exception ex)
                {
                Log($"Failed to insert patient data: {ex.Message}, InnerException: {ex.InnerException?.Message}");
                throw new Exception($"Failed to insert patient data: {ex.Message}", ex);
            }
        }

        public static void UpdateValueToDb(string value, string fieldName)
        {
             // Log($"Database disabled. Skipping UpdateValueToDb: {fieldName}={value}");
             return;
        }

        public static DataTable ReadValueFromDb(string paramList)
        {
             // Log($"Database disabled for Settings. Skipping ReadValueFromDb: {paramList}");
             return null;
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

        internal static void Log(string message)
        {
            Debug.WriteLine(message);
        }

        internal class SerialPortInterface
        {
            private static SerialPort _myCOMPort = new SerialPort();
            public static event Action<string> OnDataReceived;
            private static CancellationTokenSource _pollingCts;
            private static readonly object _writeLock = new object();

            public static bool Initialize(string portName, int baudRate = 115200)
            {
                try
                {
                    if (_myCOMPort.IsOpen)
                    {
                        Log($"Serial port {portName} is already open.");
                        return true;
                    }

                    _myCOMPort.PortName = portName;
                    _myCOMPort.BaudRate = baudRate;

                    _myCOMPort.DataReceived += (s, e) =>
                    {
                        try
                        {
                            //string rawData = _myCOMPort.ReadExisting();
                            //Log($"{rawData}||||||||||||||||||||||||");
                            try 
                            {
                                string data = _myCOMPort.ReadLine();
                                Log($"Serial data received: {data}");
                                DataProcessor.ProcessData(data);
                                OnDataReceived?.Invoke(data);
                            }
                            catch (Exception readlineEx)
                            {
                                // Log($"ReadLine error (can happen during close/disconnect): {readlineEx.Message}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log($"Serial read failed: {ex.Message}");
                        }
                    };

                    _myCOMPort.Open();
                    Log($"Serial port {portName} opened.");
                    
                    StartPolling();

                    return true;
                }
                catch (Exception ex)
                {
                    Log($"Serial port initialization failed: {ex.Message}");
                    return false;
                }
            }

            public static void Write(string data)
            {
                try
                {
                    if (string.IsNullOrEmpty(data)) return;

                    // Ensure command ends with newline as per hardware requirement
                    if (!data.EndsWith("\n"))
                    {
                        data += "\n";
                    }

                    if (!_myCOMPort.IsOpen)
                    {
                        Log($"Opening port {_myCOMPort.PortName}...");
                        _myCOMPort.Open();
                    }

                    lock (_writeLock)
                    {
                        _myCOMPort.Write(data);
                    }
                    Log($"Data written to port: {data.TrimEnd()} AT {_myCOMPort.PortName}");
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
                    StopPolling();
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

            private static void StartPolling()
            {
                StopPolling(); // Stop any existing polling
                _pollingCts = new CancellationTokenSource();
                Task.Run(async () => await PollingLoop(_pollingCts.Token));
            }

            private static void StopPolling()
            {
                if (_pollingCts != null)
                {
                    _pollingCts.Cancel();
                    _pollingCts.Dispose();
                    _pollingCts = null;
                }
            }

            private static async Task PollingLoop(CancellationToken token)
            {
                /* 
                string[] commands = new[] { "GET_DHT", "GET_HEPA", "GET_DIFF", "GET_GAS", "GET_UPS" };
                int index = 0;
                
                // Allow some start up time
                await Task.Delay(1000, token);

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        if (_myCOMPort != null && _myCOMPort.IsOpen)
                        {
                            var cmd = new { cmd = commands[index] };
                            string jsonCmd = JsonSerializer.Serialize(cmd);
                            Write(jsonCmd + "\n"); 

                            index = (index + 1) % commands.Length;
                        }
                        else
                        {
                            // If port closed, wait a bit longer before checking again
                            await Task.Delay(1000, token);
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"Polling error: {ex.Message}");
                    }

                    try 
                    {
                        await Task.Delay(800, token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
                */
                await Task.CompletedTask;
            }
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

            public static event Action<bool> onGeneralLight1Updated;
            public static event Action<bool> onGeneralLight2Updated;
            public static event Action<bool> onLaminarLightUpdated;
            public static event Action<bool> onOTLight1Updated;
            public static event Action<bool> onOTLight2Updated;
            public static event Action<bool> onTempUpdatedByController;
            public static event Action<bool> onHumdUpdatedByController;

            // Intercom Events
            public static event Action OnRinging;
            public static event Action OnHookUp;
            public static event Action OnHookDown;
            public static event Action<string> OnVolumeChanged;
            public static event Action OnOutgoingCall;


            public static void ProcessData(string inData)
            {
                if (string.IsNullOrWhiteSpace(inData)) return;

                try 
                {
                    // Trying to parse as generic JSON object first to check for "sensor", "cmd", or "error" keys
                    var jsonDoc = JsonDocument.Parse(inData);
                    var root = jsonDoc.RootElement;

                    if (root.TryGetProperty("error", out var errorProp))
                    {
                        Log($"Device reported error: {errorProp.GetString()}");
                        return;
                    }

                    // Check for sensor responses
                    if (root.TryGetProperty("sensor", out var sensorProp))
                    {
                        // ... existing sensor logic ...
                        string sensor = sensorProp.GetString();
                        switch (sensor)
                        {
                            case "dht11":
                            {
                                if (root.TryGetProperty("temp", out var tempEl) && root.TryGetProperty("humi", out var humiEl))
                                {
                                    // Robust parsing (String or Number)
                                    double tempVal = 0;
                                    double humiVal = 0;

                                    if (tempEl.ValueKind == JsonValueKind.Number) tempVal = tempEl.GetDouble();
                                    else if (tempEl.ValueKind == JsonValueKind.String) double.TryParse(tempEl.GetString(), out tempVal);

                                    if (humiEl.ValueKind == JsonValueKind.Number) humiVal = humiEl.GetDouble();
                                    else if (humiEl.ValueKind == JsonValueKind.String) double.TryParse(humiEl.GetString(), out humiVal);

                                    // DIVIDE BY 10 RE-ENABLED AS PER USER REQUEST
                                    tempVal /= 10.0;
                                    humiVal /= 10.0;

                                    string temp = tempVal.ToString("F1");
                                    string humi = humiVal.ToString("F1");
                                    
                                    SystemInfo.Temperature = temp;
                                    SystemInfo.Humidity = humi;
                                    
                                    Log($"DHT: Temp={temp}, Humi={humi}");
                                    OnTemperatureUpdated?.Invoke(temp);
                                    OnHumidityUpdated?.Invoke(humi);
                                }
                                break;
                            }
                            case "hepa":
                            {
                                // hepa_hpa
                                break;
                            }
                            case "diff":
                            {
                                if (root.TryGetProperty("diff_hpa", out var diffEl))
                                {
                                    // Robust parsing
                                    string diff = diffEl.ValueKind == JsonValueKind.String ? diffEl.GetString() : diffEl.ToString();
                                    
                                    SystemInfo.AirDiffPress = diff;
                                    Log($"DIFF: {diff} hPa");
                                    onAirDiffPressureUpdated?.Invoke(diff);
                                }
                                break;
                            }
                            case "gas_array":
                            {
                                ProcessGasArray(root);
                                break;
                            }
                        }
                    }
                    // Check for device responses (e.g. UPS)
                    else if (root.TryGetProperty("device", out var deviceProp))
                    {
                         if (deviceProp.GetString() == "UPS")
                         {
                             if (root.TryGetProperty("state", out var stateProp))
                             {
                                 bool isOn = stateProp.GetString() == "ON";
                                 Log($"UPS State: {isOn}");
                                 OnUpsStatusUpdated?.Invoke(isOn);
                             }
                         }
                    }
                    // Check for commands (ACKs or Events)
                    else if (root.TryGetProperty("cmd", out var cmdProp))
                    {
                        string cmd = cmdProp.GetString();
                        
                        if (cmd == "SET_AHU")
                        {
                            // Acknowledge logic if needed
                        }
                        else if (cmd.StartsWith("SET_GEN") || cmd.StartsWith("SET_OT") || cmd == "SET_LAM")
                        {
                            ProcessLightAck(cmd, root);
                        }
                        // Intercom Commands/Events
                        else if (cmd == "RINGING")
                        {
                            Log("Intercom: RINGING");
                            OnRinging?.Invoke();
                        }
                        else if (cmd == "HKU")
                        {
                            Log("Intercom: Hook Up");
                            OnHookUp?.Invoke();
                        }
                        else if (cmd == "HKD")
                        {
                            Log("Intercom: Hook Down");
                            OnHookDown?.Invoke();
                        }
                        else if (cmd == "VOLUME")
                        {
                             if (root.TryGetProperty("val", out var valProp))
                             {
                                 string val = valProp.ValueKind == JsonValueKind.String ? valProp.GetString() : valProp.ToString();
                                 Log($"Intercom: Volume {val}");
                                 OnVolumeChanged?.Invoke(val);
                             }
                        }
                         else if (cmd == "CALL")
                        {
                            Log("Intercom: Outgoing Call");
                            OnOutgoingCall?.Invoke();
                        }
                    }
                }
                catch (JsonException ex)
                {
                   Log($"JSON Parse Error: {ex.Message}. Data: {inData}");
                }
                catch (Exception ex)
                {
                   Log($"ProcessData Error: {ex.Message}");
                }
            }

            private static void ProcessGasArray(JsonElement root)
            {
                // "o2": "HIGH", "n2": "LOW", "co2": "MED", "air4": "LOW", "air7": "HIGH", "vac": "MED"
                // Mapping to existing events. 
                // Existing logic expected pressure strings for 'RDGA' etc, and boolean alerts for 'ALGA' etc.
                // The new protocol sends HIGH/MED/LOW.
                // We will update SystemInfo with these strings.
                
                UpdateGas("o2", "O₂", root);
                UpdateGas("n2", "N₂O", root); // Assuming 'n2' maps to N2O as per old code's RDGB
                UpdateGas("co2", "CO₂", root);
                UpdateGas("air4", "AIR 4", root);
                UpdateGas("air7", "AIR 7", root);
                UpdateGas("vac", "VAC", root);
            }

            private static void UpdateGas(string jsonKey, string displayName, JsonElement root)
            {
                if (root.TryGetProperty(jsonKey, out var valProp))
                {
                    string status = valProp.GetString(); // HIGH, MED, LOW
                    // Map to SystemInfo. The old code stored numeric strings or "0".
                    // We will store the status string for now.
                    
                    bool isAlert = status == "HIGH" || status == "LOW"; // Assuming MED is normal?
                    // Actually, the requirement says "Retrieves the status...".
                    // The old code had separate pressure updates (RDGA) and Alert updates (ALGA/BLGA).
                    // We can emit both.
                    
                    OnGasPressureUpdated?.Invoke(displayName, status);
                    
                    // Logic for alert: If HIGH or LOW, maybe trigger alert? 
                    // Let's assume HIGH/LOW is bad and MED is good for now, 
                    // OR just pass the status. The old code had 'isAlert' bool.
                    // Let's trigger alert if not MED?
                    // The user prompt doesn't strictly specify what constitutes a "Medical Gas Alert" in the new protocol,
                    // but usually HIGH/LOW are abnormal.
                    
                    if (status != "MED")
                    {
                         OnGasAlertUpdated?.Invoke(displayName, true);
                    }
                    else
                    {
                         OnGasAlertUpdated?.Invoke(displayName, false);
                    }

                    // Update static properties
                    switch(displayName)
                    {
                        case "O₂": SystemInfo.Oxygen = status; break;
                        case "N₂O": SystemInfo.Nitrogen = status; break;
                        case "CO₂": SystemInfo.CO2 = status; break;
                        case "AIR 4": SystemInfo.Air4 = status; break;
                        case "AIR 7": SystemInfo.Air7 = status; break;
                        case "VAC": SystemInfo.Vacuum = status; break;
                    }
                }
            }

            private static void ProcessLightAck(string cmd, JsonElement root)
            {
                 bool stateFound = root.TryGetProperty("state", out var stateProp);
                 bool valFound = root.TryGetProperty("val", out var valProp);

                 // 1. Get current state as baseline
                 bool isOn = false;
                 int intensity = 0;
                 if (cmd == "SET_GEN1") { isOn = SystemInfo.GeneralLight1On; intensity = SystemInfo.GeneralLight1Intensity; }
                 else if (cmd == "SET_GEN2") { isOn = SystemInfo.GeneralLight2On; intensity = SystemInfo.GeneralLight2Intensity; }
                 else if (cmd == "SET_LAM") { isOn = SystemInfo.LaminarLightOn; intensity = SystemInfo.LaminarLightIntensity; }
                 else if (cmd == "SET_OT1") { isOn = SystemInfo.OTLight1On; }
                 else if (cmd == "SET_OT2") { isOn = SystemInfo.OTLight2On; }

                 // 2. Parse intensity if provided
                 if (valFound)
                 {
                     if (valProp.ValueKind == JsonValueKind.Number) intensity = valProp.GetInt32();
                     else int.TryParse(valProp.GetString(), out intensity);
                 }

                 // 3. Apply Priority Logic:
                 // Changed per user request: "status": "ACK" + "val" is the source of truth.
                 // "state" is NOT sent or should be ignored.
                 // 0 = OFF, >0 = ON.

                 if (valFound)
                 {
                     isOn = intensity > 0;
                 }
                 else if (stateFound) // Fallback just in case old firmware is used
                 {
                     isOn = stateProp.GetString() == "ON";
                 }

                 if (cmd == "SET_GEN1")
                 {
                     SystemInfo.GeneralLight1On = isOn;
                     SystemInfo.GeneralLight1Intensity = intensity;
                     onGeneralLight1Updated?.Invoke(isOn); 
                 }
                 else if (cmd == "SET_GEN2")
                 {
                     SystemInfo.GeneralLight2On = isOn;
                     SystemInfo.GeneralLight2Intensity = intensity;
                     onGeneralLight2Updated?.Invoke(isOn);
                 }
                 else if (cmd == "SET_LAM")
                 {
                     SystemInfo.LaminarLightOn = isOn;
                     SystemInfo.LaminarLightIntensity = intensity;
                     onLaminarLightUpdated?.Invoke(isOn);
                 }
                 else if (cmd == "SET_OT1")
                 {
                     SystemInfo.OTLight1On = isOn;
                     onOTLight1Updated?.Invoke(isOn);
                 }
                 else if (cmd == "SET_OT2")
                 {
                     SystemInfo.OTLight2On = isOn;
                     onOTLight2Updated?.Invoke(isOn);
                 }
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
        public static string AirDiffPress { get; set; } = "0";
        public static string Temperature { get; set; } = "0";
        public static string TemperatureSetValue { get; set; } = "0";
        public static string Humidity { get; set; } = "0";
        public static string Voltage { get; set; } = "0";
        public static string Current { get; set; } = "0";

        // Light States (Transient)
        public static bool GeneralLight1On { get; set; }
        public static int GeneralLight1Intensity { get; set; }
        public static bool GeneralLight2On { get; set; }
        public static int GeneralLight2Intensity { get; set; }
        public static bool LaminarLightOn { get; set; }
        public static int LaminarLightIntensity { get; set; }
        public static bool OTLight1On { get; set; }
        public static bool OTLight2On { get; set; }
    }
}
