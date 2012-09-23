using System;

namespace Columbia.Scheduler
{
    public static class Logger
    {
        public static T Dump<T>(this T obj, string message = null) where T : class
        {
            if (message != null)
            {
                Console.WriteLine(message);
            }
            Console.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss "));
            Console.WriteLine(obj == null ? "* null *" : obj.ToString());
            return obj;
        }
    }
}
