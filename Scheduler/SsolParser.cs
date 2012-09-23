using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using HtmlAgilityPack;

namespace Columbia.Scheduler
{
    public class SsolParser : IRegistrationParser
    {
        public int MsToSleepBetweenTries
        {
            get { return 2000; } //ssol will block your ip pretty quick if you do <1000ms
        }

        public int MaxLoginAttempts
        {
            get { return 2; } //dont want to blow up ssol
        }

        public string GetInitUrl(string callNumber)
        {
            var formAction = string.Format("{0}/{1}/", ParserConstants.FormStart, ParserConstants.RandomInitSession);
            return string.Format(ParserConstants.RootUrlFmt, formAction) + string.Format(ParserConstants.InitParamsFmt, callNumber);
        }

        public Tuple<string, NameValueCollection> GetAddRequest(string html)
        {
            var content = GetRegistrationContentNode(html);
            var forms = GetFormsFromContentNode(content);

            var finalAddForm = forms.First(x => x.FormNode.GetAttributeValue("action", "").StartsWith(ParserConstants.FormStart)
                                             && x.FormNode.GetAttributeValue("name", "").StartsWith(ParserConstants.FinalAddFormName));

            var formData = new NameValueCollection();

            foreach (var input in finalAddForm.Inputs)
            {
                if (!string.IsNullOrEmpty(input.Value))
                {
                    formData[input.Key] = input.Value;
                }
            }

            if (finalAddForm.Selects.Count > 0)
            {
                if (finalAddForm.Selects.Count == 1 && finalAddForm.Selects.First().Key.Equals(ParserConstants.SelectPassFailKey))
                {
                    formData[ParserConstants.SelectPassFailKey] = ParserConstants.PassFailResponse;
                }
                else
                {
                    throw new Exception("Could not fill out all selectable options automatically");
                }
            }

            var url = string.Format(ParserConstants.RootUrlFmt, finalAddForm.ActionUrl).TrimEnd('?');

            return Tuple.Create(url, formData);
        }

        public Response ParseResponse(string html, string username, string password)
        {
            var content = GetRegistrationContentNode(html);
            var allForms = GetFormsFromContentNode(content);
            var forms = allForms.Where(x => x.FormNode.GetAttributeValue("action", "").StartsWith(ParserConstants.FormStart))
                .ToList();

            if (!forms.Any())
            {
                throw new Exception("No forms to submit!");
            }

            var searchForms = forms.Where(x => x.FormNode.GetAttributeValue("name", "").Equals(ParserConstants.FormName))
                .ToList();

            var formToSubmit = searchForms.FirstOrDefault() ?? forms.First();
            var formToSubmitIfAdding = searchForms.LastOrDefault();

            var resp = new Response { NextUrl = string.Format(ParserConstants.RootUrlFmt, formToSubmit.ActionUrl) };

            if (content.OuterHtml.Contains(ParserConstants.SysNotAvail))
            {
                throw new Exception("System is not available!");
            }
            
            if (content.OuterHtml.Contains(ParserConstants.NoAvailSections))
            {
                resp.Type = GetNoneAvailResponseType(content);
            }

            if (formToSubmitIfAdding != null)
            {
                var hasAddButton = formToSubmitIfAdding.Inputs.Any(x => x.Key.Equals(ParserConstants.AddSubmitKey));
                if (hasAddButton)
                {
                    formToSubmit = formToSubmitIfAdding;
                    resp.Type = ResponseType.Open;
                }
            }

            foreach (var input in formToSubmit.Inputs)
            {
                if (!string.IsNullOrEmpty(input.Value))
                {
                    resp.NextUrl += input.Key + "=" + input.Value + "&";
                }
                else
                {
                    // needs to be filled

                    if (input.Key.Equals(ParserConstants.NeedsPasswordInput))
                    {
                        resp.Type = ResponseType.NeedToLogIn;
                        resp.NextUrl += input.Key + "=" + password + "&";
                    }
                    else if (input.Key.Equals(ParserConstants.NeedsUsernameInput))
                    {
                        resp.Type = ResponseType.NeedToLogIn;
                        resp.NextUrl += input.Key + "=" + username + "&";
                    }
                    else
                    {
                        throw new Exception("Unhandled empty input: " + input.Key);
                    }
                }
            }

            if (formToSubmit.Selects.Count > 0)
            {
                throw new Exception("Cannot submit form because options need to be selected manually!");
            }

            return resp;
        }

        public ResponseType GetNoneAvailResponseType(HtmlNode content)
        {
            HtmlNode reason;
            var restrictedCourseRow = content.SelectNodes("//tr[@class='" + ParserConstants.RestrictedClass + "']");
            var blockedCourseRow = content.SelectNodes("//tr[@class='" + ParserConstants.BlockedClass + "']");

            if (restrictedCourseRow != null && restrictedCourseRow.Count > 0)
            {
                reason = restrictedCourseRow.First().LastChild;
            }
            else if (blockedCourseRow != null && blockedCourseRow.Count > 0)
            {
                reason = blockedCourseRow.First().LastChild;
            }
            else
            {
                throw new Exception("No avail sections, but couldn't find course row");
            }

            if (reason.OuterHtml.Contains("Conflict with"))
            {
                return ResponseType.Conflict;
            }

            if (reason.OuterHtml.Contains("Restricted"))
            {
                return ResponseType.Restricted;
            }

            if (reason.OuterHtml.Contains("Registered"))
            {
                return ResponseType.Registered;
            }

            if (reason.OuterHtml.Contains("Full"))
            {
                return ResponseType.Full;
            }

            if (reason.OuterHtml.Contains("Blocked"))
            {
                return ResponseType.Blocked;
            }

            throw new Exception("Unhandled conflict case: " + reason.OuterHtml);
        }

        public HtmlNode GetRegistrationContentNode(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var content = doc.GetElementbyId("Content");
            if (content == null)
            {
                throw new Exception("Yikes, might've been blocked by ssol!");
            }
            return content;
        }

        //tags are closed weird, can't just get descendants
        public IEnumerable<HtmlForm> GetFormsFromContentNode(HtmlNode content)
        {
            var forms = new List<HtmlForm>();

            foreach (var child in content.DescendantNodes())
            {
                if (child.Name.Equals("form"))
                {
                    forms.Add(new HtmlForm { FormNode = child, ActionUrl = child.GetAttributeValue("action", "") });
                }
                else if (child.Name.Equals("input"))
                {
                    var key = child.GetAttributeValue("name", null);
                    var value = child.GetAttributeValue("value", null);

                    if (!string.IsNullOrEmpty(key) && !key.Equals("reset"))
                    {
                        forms.Last().Inputs[key] = value;
                    }
                }
                else if (child.Name.Equals("select"))
                {
                    var key = child.GetAttributeValue("name", null);
                    var options = child.Descendants("option")
                        .Select(y => y.GetAttributeValue("value", null))
                        .Where(y => !string.IsNullOrEmpty(y))
                        .ToList();

                    if (!string.IsNullOrEmpty(key))
                    {
                        forms.Last().Selects[key] = options;
                    }
                }
            }

            return forms;
        }
    }
}