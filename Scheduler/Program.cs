using System;

namespace Columbia.Scheduler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var callNumber = GetCallNumber(args);

            const string uName = "usernameHere";
            const string pWord = "passwordHere";

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

        private static string GetCallNumber(string[] args)
        {
            var callNumber = "78696";

            if (args.Length > 0)
            {
                var type = (ResponseType)Enum.Parse(typeof(ResponseType), args[0]);

                switch (type)
                {
                    case ResponseType.Blocked:
                        callNumber = "72208";
                        break;
                    case ResponseType.Conflict:
                        callNumber = "dontknow";
                        break;
                    case ResponseType.Open:
                        callNumber = "05362";
                        break;
                    case ResponseType.Restricted:
                        callNumber = "16598";
                        break;
                    case ResponseType.Registered:
                        callNumber = "78696";
                        break;
                }
            }

            return callNumber;
        }
    }
}
