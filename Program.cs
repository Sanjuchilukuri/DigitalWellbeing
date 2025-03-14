//using System.Runtime.InteropServices;
//using System.Text;

//namespace DigitalWellbeing
//{
//    class Program
//    {
//        [DllImport("user32.dll")]
//        static extern IntPtr GetForegroundWindow();

//        [DllImport("user32.dll")]
//        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

//        static string GetActiveWindowTitle()
//        {
//            const int nChars = 256;
//            StringBuilder buff = new StringBuilder(nChars);
//            IntPtr handle = GetForegroundWindow();

//            if (GetWindowText(handle, buff, nChars) > 0)
//                return buff.ToString();

//            return "Unknown";
//        }

//        static void Main()
//        {
//            string currentApp = "";
//            DateTime startTime = DateTime.Now;

//            while (true)
//            {
//                string activeApp = GetActiveWindowTitle();

//                if (activeApp != currentApp)
//                {
//                    if (!string.IsNullOrEmpty(currentApp))
//                    {
//                        LogAppUsage(currentApp, startTime, DateTime.Now);
//                    }

//                    currentApp = activeApp;
//                    startTime = DateTime.Now;
//                }

//                Thread.Sleep(1000); 
//            }
//        }

//        static void LogAppUsage(string appName, DateTime start, DateTime end)
//        {
//            appName = appName.ToString().Split("\\")[appName.ToString().Split("\\").Length - 1];

//            if (appName.Contains("-"))
//            {
//                appName = appName.Substring(appName.LastIndexOf("-") + 1);
//            }

//            if (appName.Contains("\\"))
//            {
//                appName = appName.Substring(appName.LastIndexOf("\\") + 1);
//            }

//            if (appName.Contains(":"))
//            {
//                appName = appName.Substring(appName.LastIndexOf(":") + 1);
//            }

//            if (appName.Contains("|"))
//            {
//                appName = appName.Substring(appName.LastIndexOf("|") + 1);
//            }

//            string logMessage = $"Logged: {appName} | Duration: {end - start}";

//            string filePath = "C:\\PROJ\\DigitalWellbeing\\log.txt";

//            using (StreamWriter writer = new StreamWriter(filePath, true))
//            {
//                writer.WriteLine(logMessage);
//            }
//        }

//        static void ProcessLogs()
//        {
//            string filePath = "C:\\PROJ\\DigitalWellbeing\\log.txt";

//            Dictionary<string, TimeSpan> appDurations = new Dictionary<string, TimeSpan>();


//            foreach (var line in File.ReadLines(filePath))
//            {
//                var lastPipeIndex = line.LastIndexOf("|");

//                var text0 = line.Substring(0, lastPipeIndex);
//                var text1 = line.Substring(lastPipeIndex + 1);

//                var duration = text1.Substring(text1.IndexOf(":") + 1);
//                var time = TimeSpan.Parse(duration);

//                if (text0.Contains("-"))
//                {
//                    text0 = text0.Substring(text0.LastIndexOf("-") + 1);
//                }

//                if (text0.Contains("\\"))
//                {
//                    text0 = text0.Substring(text0.LastIndexOf("\\") + 1);
//                }

//                if (text0.Contains(":"))
//                {
//                    text0 = text0.Substring(text0.LastIndexOf(":") + 1);
//                }

//                if (text0.Contains("|"))
//                {
//                    text0 = text0.Substring(text0.LastIndexOf("|") + 1);
//                }

//                if (appDurations.ContainsKey(text0.Trim()))
//                {
//                    appDurations[text0.Trim()] += time;
//                }
//                else
//                {
//                    appDurations[text0.Trim()] = time;
//                }
//            }


//            //i want to store these dict into db
//            //UUID , Date, AppName, Duration
//            SaveToDatabase();
//        }


//    }

//}

using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Text;

namespace DigitalWellbeing
{
    class Program
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        static string connectionString = "Data Source=L-IT-0177\\PRAVAL;database=DigitalWellbeing; Trusted_Connection=True; encrypt=True; trustservercertificate=True";
        static string filePath = "C:\\PROJ\\DigitalWellbeing\\log.txt";

        static Dictionary<string, TimeSpan> appDurations = new Dictionary<string, TimeSpan>();

        static void Main()
        {
            SystemEvents.PowerModeChanged += async (sender, e) => await OnPowerModeChanged(sender, e);
            SystemEvents.SessionEnding += async (sender, e) => await OnSessionEnding(sender, e);
            SystemEvents.EventsThreadShutdown += async (sender, e) => await ProcessLogs();
            SystemEvents.SessionEnded += async (sender, e) => await ProcessLogs();

            string currentApp = "";
            DateTime startTime = DateTime.Now;

            while (true)
            {
                string activeApp = GetActiveWindowTitle();

                if (activeApp != currentApp)
                {
                    if (!string.IsNullOrEmpty(currentApp))
                    {
                        LogAppUsage(currentApp, startTime, DateTime.Now);
                    }

                    currentApp = activeApp;
                    startTime = DateTime.Now;
                }

                Thread.Sleep(1000);
            }
        }

        static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, buff, nChars) > 0)
                return buff.ToString();

            return "Unknown";
        }

        static void LogAppUsage(string appName, DateTime start, DateTime end)
        {
            string cleanAppName = ExtractAppName(appName);
            string logMessage = $"Logged: {cleanAppName} | Duration: {end - start}";

            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(logMessage);
            }
        }

        static string ExtractAppName(string appName)
        {
            if (appName.Contains("-"))
                appName = appName.Substring(appName.LastIndexOf("-") + 1);

            if (appName.Contains("\\"))
                appName = appName.Substring(appName.LastIndexOf("\\") + 1);

            if (appName.Contains(":"))
                appName = appName.Substring(appName.LastIndexOf(":") + 1);

            if (appName.Contains("|"))
                appName = appName.Substring(appName.LastIndexOf("|") + 1);

            return appName.Trim();
        }

        static async Task ProcessLogs()
        {
            appDurations.Clear();

            foreach (var line in File.ReadLines(filePath))
            {
                var lastPipeIndex = line.LastIndexOf("|");
                var text0 = line.Substring(0, lastPipeIndex);
                var text1 = line.Substring(lastPipeIndex + 1);

                var duration = text1.Substring(text1.IndexOf(":") + 1);
                var time = TimeSpan.Parse(duration);

                text0 = ExtractAppName(text0);

                if (appDurations.ContainsKey(text0))
                    appDurations[text0] += time;
                else
                    appDurations[text0] = time;
            }

            await SaveToDatabase();
            await ClearLogFile();
        }

        static async Task ClearLogFile()
        {
            string filePath = "C:\\PROJ\\DigitalWellbeing\\log.txt";

            if (File.Exists(filePath))
            {
                await File.WriteAllTextAsync(filePath, string.Empty); 
            }
            else
            {
                Console.WriteLine("Log file does not exist.");
            }
        }


        static async Task SaveToDatabase()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                foreach (var entry in appDurations)
                {
                    string query = "INSERT INTO AppUsage (Date, AppName, Duration) VALUES (@date, @appName, @duration)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        //cmd.Parameters.AddWithValue("@date", DateTime.Now.Date);
                        cmd.Parameters.AddWithValue("@date", DateTime.Now.Date.AddDays(-1));
                        cmd.Parameters.AddWithValue("@appName", entry.Key);
                        cmd.Parameters.AddWithValue("@duration", entry.Value);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        static async Task OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend)
            {
                Console.WriteLine("System going to sleep, processing logs...");
                await ProcessLogs();
            }
        }

        static async Task OnSessionEnding(object sender, SessionEndingEventArgs e)
        {
            Console.WriteLine("System shutting down, processing logs...");
            await ProcessLogs();
        }
    }
}
