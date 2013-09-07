using System.Collections.Generic;
using HtmlAgilityPack;

namespace DegreeAuditReport
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
}
