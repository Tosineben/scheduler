using System;

namespace Scheduler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const string callNumber = "16338";
            const string uName = "usernamehere";
            const string pWord = "passwordhere";

            try
            {
                new Scheduler().RegisterForClass(uName, pWord, callNumber);                
            }
            catch(Exception e)
            {
                Console.Write(e.Message);
            }

            Console.Write("Done!");
            Console.ReadKey();
        }
    }
}
