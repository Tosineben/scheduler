using System;
using System.Net;
using System.Text;
using System.Threading;

namespace Columbia.Scheduler
{
    public class Scheduler
    {
        private readonly IRegistrationParser _parser;
        private readonly WebClient _wc;

        public Scheduler(IRegistrationParser parser, WebClient wc)
        {
            _parser = parser;
            _wc = wc;
        }

        public Scheduler() : this (new SsolParser(), new WebClient()) {}

        public void RegisterForClass(string username, string password, string callNumber)
        {
            var passwordTries = 0;

            var response = new Response { NextUrl = _parser.GetInitUrl(callNumber) };

            while (true)
            {
                if (passwordTries >= _parser.MaxLoginAttempts)
                {
                    throw new Exception("Tried to enter password too many times!");
                }

                var nextHtml = _wc.DownloadString(response.NextUrl);
                response = _parser.ParseResponse(nextHtml, username, password);

                if (response.Type == ResponseType.Registered)
                {
                    "Already registered, awesome!".Dump();
                    return;
                }

                if (response.Type == ResponseType.Open)
                {
                    "*********** Course is open ***********".Dump();
                    DoAddRequest(response.NextUrl);
                    return;
                }

                if (response.Type == ResponseType.Conflict)
                {
                    "Course conflicts with another course of yours, fail!".Dump();
                    return;
                }

                if (response.Type == ResponseType.Restricted)
                {
                    "Course is restricted, fail!".Dump();
                    return;
                }

                if (response.Type == ResponseType.Blocked)
                {
                    "Couse is blocked, fail!".Dump();
                    return;
                }

                if (response.Type == ResponseType.NeedToLogIn)
                {
                    "Session expired, entering password".Dump();
                    passwordTries++;
                }
                else if (response.Type == ResponseType.Full)
                {
                    "Course is full :(".Dump();
                }
                else
                {
                    "Failed to parse response".Dump();
                }

                ".......".Dump();
                Thread.Sleep(_parser.MsToSleepBetweenTries);
            }
        }

        public void DoAddRequest(string initUrl)
        {
            var html = _wc.DownloadString(initUrl);
            var urlAndData = _parser.GetAddRequest(html);
            var respBytes = _wc.UploadValues(urlAndData.Item1, urlAndData.Item2);
            var resp = Encoding.UTF8.GetString(respBytes);
            resp.Dump("Response after submitting add request");
        }
    }
}