using System;
using System.Collections.Specialized;

namespace Scheduler
{
    public interface IRegistrationParser
    {
        int MsToSleepBetweenTries { get; }
        int MaxLoginAttempts { get; }

        string GetInitUrl(string callNumber);
        Tuple<string, NameValueCollection> GetAddRequest(string html);
        Response ParseResponse(string html, string username, string password);
    }
}