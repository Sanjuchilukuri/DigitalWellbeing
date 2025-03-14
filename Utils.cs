namespace DigitalWellbeing
{
    public class Utils
    {
  
        public string ExtractAppName(string appName)
        {
            if (appName.Contains("-"))
                appName = appName.Substring(appName.LastIndexOf("-") + 1);

            if (appName.Contains("\\"))
                appName = appName.Substring(appName.LastIndexOf("\\") + 1);

            if (appName.Contains(":"))
                appName = appName.Substring(appName.LastIndexOf(":") + 1);

            if (appName.Contains("|"))
                appName = appName.Substring(appName.LastIndexOf("|") + 1);

            if (appName.Contains(".exe"))
                appName = appName.Replace(".exe", "");
            return appName.Trim();
        }


        
    }
}
