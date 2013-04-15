using System;
using System.IO;

namespace Scheduler
{
    public static class Logger
    {
        static Logger()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var folder = Path.Combine(appDataPath, "Emdaq", "Scheduler");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            _logFile = Path.Combine(folder, string.Format("log-{0}.txt", DateTime.Now.Ticks));
        }

        private static readonly string _logFile;

        public static T Dump<T>(this T obj, string message = null) where T : class
        {
            if (message != null)
            {
                Log(message);
            }
            Log(obj == null ? "* null *" : obj.ToString());
            return obj;
        }

        public static void Log(string message)
        {
            Console.WriteLine(message);
            var logMsg = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss - ") + message + "\n";
            File.AppendAllText(_logFile, logMsg);
        }
    }
}
