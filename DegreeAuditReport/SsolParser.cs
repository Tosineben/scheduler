using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using HtmlAgilityPack;

namespace DegreeAuditReport
{
    public class SsolParser
    {
        public string GetDegreeAuditReport(string fullCopy)
        {
            return fullCopy.Substring(fullCopy.IndexOf("PREPARED: "));
        }

        public string GetDegreeAuditReport(string username, string password)
        {
            var url = ParserConstants.SSOL_BASE_URL + _currentMainUrl;
            var html = _wc.DownloadString(url);
            var content = EnsureLoggedIn(html, username, password);
            var pre = content.ChildNodes.Single(x => x.Name.Equals("pre"));
            return pre.InnerHtml;
        }

        private readonly WebClient _wc = new WebClient();
        private string _currentMainUrl = string.Format(ParserConstants.SSOL_MAIN_FMT, ParserConstants.RANDOM_INIT_SESSION);

        private string GetLoginUrl(HtmlNode content, string username, string password)
        {
            var allForms = GetFormsFromContentNode(content);

            foreach (var form in allForms)
            {
                var needToLogin = false;
                var loginUrl = form.ActionUrl;

                foreach (var input in form.Inputs)
                {
                    if (!string.IsNullOrEmpty(input.Value))
                    {
                        loginUrl += input.Key + "=" + input.Value + "&";
                    }
                    else
                    {
                        // needs to be filled

                        if (input.Key.Equals(ParserConstants.NEED_PASSWORD_INPUT))
                        {
                            needToLogin = true;
                            loginUrl += input.Key + "=" + password + "&";
                        }
                        else if (input.Key.Equals(ParserConstants.NEED_USERNAME_INPUT))
                        {
                            needToLogin = true;
                            loginUrl += input.Key + "=" + username + "&";
                        }
                        else
                        {
                            throw new Exception("Unhandled empty input: " + input.Key);
                        }
                    }
                }

                if (needToLogin)
                {
                    _currentMainUrl = form.ActionUrl;
                    return loginUrl;
                }
            }

            return null;
        }

        private HtmlNode EnsureLoggedIn(string html, string username, string password)
        {
            var content = GetContentNode(html);
            var loginUrl = GetLoginUrl(content, username, password);

            if (loginUrl == null)
            {
                return content;
            }

            var newHtml = _wc.DownloadString(ParserConstants.SSOL_BASE_URL + loginUrl);
            var newContent = GetContentNode(newHtml);

            var newLoginUrl = GetLoginUrl(content, username, password);

            if (newLoginUrl == null)
            {
                return newContent;
            }

            throw new Exception("failed to log in");
        }

        private HtmlNode GetContentNode(string html)
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

        private IEnumerable<HtmlForm> GetFormsFromContentNode(HtmlNode content)
        {
            var forms = new List<HtmlForm>();

            foreach (var child in GetDescendants(content))
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

        //tags are closed weird, can't just use node.Descendants()
        private IEnumerable<HtmlNode> GetDescendants(HtmlNode node)
        {
            foreach (var child in node.ChildNodes)
            {
                yield return child;
                foreach (var subChild in GetDescendants(child))
                {
                    yield return subChild;
                }
            }
        }
    }
}