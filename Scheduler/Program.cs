using System;

namespace Scheduler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const string uName = "usernameHere";
            const string pWord = "passwordHere";

            try
            {
                var s = new Scheduler();
                s.RegisterForClass(uName, pWord, "75499");
            }
            catch(Exception e)
            {
                e.Dump();
            }

            "Done!".Dump();
            Console.ReadKey();
        }
    }
}
