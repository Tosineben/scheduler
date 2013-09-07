using System;
using System.IO;

namespace DegreeAuditReport
{
    public static class Logger
    {
        static Logger()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var folder = Path.Combine(appDataPath, "Emdaq", "DegreeAuditReport");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            LogFile = Path.Combine(folder, string.Format("log-{0}.txt", DateTime.Now.Ticks));
        }

        private static readonly string LogFile;

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
            File.AppendAllText(LogFile, logMsg);
        }
    }
}
