using System.Collections.Generic;
using HtmlAgilityPack;

namespace Columbia.Scheduler
{
    public class HtmlForm
    {
        public HtmlForm()
        {
            Inputs = new Dictionary<string, string>();
            Selects = new Dictionary<string, List<string>>();
        }

        public HtmlNode FormNode { get; set; }
        public string ActionUrl { get; set; }
        public Dictionary<string, string> Inputs { get; set; }
        public Dictionary<string, List<string>> Selects { get; set; }
    }

    public class Response
    {
        public ResponseType Type { get; set; }
        public string NextUrl { get; set; }
    }

    public enum ResponseType
    {
        None,
        NeedToLogIn,
        Full,
        Conflict,
        Restricted,
        Blocked,
        Open,
        Registered,
    }
}
