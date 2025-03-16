using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Text;

namespace DigitalWellbeing
{
    public class Listener
    {
        private readonly Logger _logger;
        private readonly Utils _utils;
        private readonly Configuration _configuration;
        private readonly DBStorage _DBStorage;


        public Listener(Logger logger, Utils utils, Configuration configuration, DBStorage dBStorage)
        {
            _logger = logger;
            _utils = utils;
            _configuration = configuration;
            _DBStorage = dBStorage;
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        static Dictionary<string, TimeSpan> appDurations = new Dictionary<string, TimeSpan>();


        public void Start()
        {
            SystemEvents.PowerModeChanged += (sender, e) => Task.Run(() => ProcessLogs());
            SystemEvents.SessionEnding += (sender, e) => Task.Run(() => ProcessLogs());
            SystemEvents.EventsThreadShutdown += (sender, e) => Task.Run(() => ProcessLogs());
            SystemEvents.SessionEnded += (sender, e) => Task.Run(() => ProcessLogs());

            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                ProcessLogs().GetAwaiter().GetResult(); 
            };


            string currentApp = "";
            DateTime startTime = DateTime.Now;

            //Task.Run(()=>ProcessLogs());

            while (true)
            {
                string activeApp = GetActiveWindowTitle();

                if (activeApp != currentApp)
                {
                    if (!string.IsNullOrEmpty(currentApp))
                    {
                        _logger.LogAppUsage(currentApp, startTime, DateTime.Now);
                    }

                    currentApp = activeApp;
                    startTime = DateTime.Now;
                }

                Thread.Sleep(1000);
            }
        }

        public string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, buff, nChars) > 0)
                return buff.ToString();

            return "Unknown";
        }

        public async Task ProcessLogs()
        {
            appDurations.Clear();

            foreach (var line in File.ReadLines(_configuration.FilePath))
            {
                var lastPipeIndex = line.LastIndexOf("|");
                var text0 = line.Substring(0, lastPipeIndex);
                var text1 = line.Substring(lastPipeIndex + 1);

                var duration = text1.Substring(text1.IndexOf(":") + 1);
                var time = TimeSpan.Parse(duration);

                text0 = _utils.ExtractAppName(text0);

                if (appDurations.ContainsKey(text0))
                    appDurations[text0] += time;
                else
                    appDurations[text0] = time;
            }

            var status = await _DBStorage.SaveToFireBase(appDurations);

            if (status)
            {
                //await _logger.ClearLogFile();
            }
        }

    }
}
