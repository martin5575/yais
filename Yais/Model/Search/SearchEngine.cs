using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using HtmlAgilityPack;
using NLog;
using System.Threading;
using Yais.Collections;
using Yais.Model.Search.ContentFinder;
using Yais.Model.Search.Robots;
using Yais.Model.Search;

namespace Yais.Model
{
    public class SearchEngine
    {
        static readonly Logger Logger = LogManager.GetLogger(nameof(SearchEngine));
        static HttpClient _client = new HttpClient();

        public SearchJob CreateSearchJob(string searchWords, int depth)
        {
            var url = new Uri($"https://www.google.de/search?q={searchWords}");

            return new SearchJob
            {
                CurrentDepth = 0,
                MaxDepth = depth,
                Link = new Link { Uri = url, Name = "Starting Point" }
            };
        }


        public async Task<SearchResult> SearchAsync(SearchJob job)
        {
            var links = new List<Link>();

            HtmlDocument html;
            try
            {
                html = await LoadHtmlAsync(job.Link.Uri);
            }
            catch(Exception exception)
            {
                Logger.Error(exception, "could not load html");
                return null;
            }

            if (job.CurrentDepth <= job.MaxDepth)
                links = ParseUris(html, job.Link.Uri);

            var items = ParseForImpressumLink(html, job.Link).ToList();

            int nextDepth = job.CurrentDepth++;
            int maxDepth = job.MaxDepth;
            return new SearchResult
            {
                Job = job,
                SubJobs = links.Select(x =>
                new SearchJob {
                    Link = x,
                    MaxDepth = maxDepth,
                    CurrentDepth = nextDepth,
                }).ToList(),
                Items = items
            };
        }

        private static async Task<HtmlDocument> LoadHtmlAsync(Uri requestUri)
        {
            Logger.Debug($"START GET {requestUri.OriginalString}");

            var content = await _client.GetStringAsync(requestUri);
            Logger.Debug($"DONE GET {requestUri.OriginalString}");

            Logger.Debug($"START PARSE HTML {requestUri.OriginalString}");
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(content);
            Logger.Debug($"DONE PARSE HTML {requestUri.OriginalString}");

            return html;
        }

        //Regex _regex = new Regex(@"\<a\ href\=""(?<url>[^""]*)"">[^\<]*\<\/a\>");

        private static Dictionary<string, RobotsHandler> _robotsHandlers = new
            Dictionary<string, RobotsHandler>();
        private RobotsHandler GetRobotsHandler(string host)
        {
            RobotsHandler result;
            if (_robotsHandlers.TryGetValue(host, out result))
                return result;

            var uri = new Uri("http://"+host + "/robots.txt", UriKind.Absolute);
            var robotsTxt = _client.GetStringAsync(uri).Result;
            result = new RobotsHandler(robotsTxt);
            _robotsHandlers.Add(host, result);

            return result;
        }


        private List<Link> ParseUris(HtmlDocument html, Uri baseUri)
        {
            var result = new List<Link>();
            try
            {
                foreach (HtmlNode node in html.DocumentNode.SelectNodes("//a[@href]"))
                {
                    var uri = GetUri(baseUri, node);
                    if (uri == null)
                        continue;

                    var robotsHandler = GetRobotsHandler(uri.Host);
                    if (robotsHandler.IsUriAllowed(uri))
                        result.Add(new Link { Uri = uri, Name = node.InnerText });
                }
            }
            catch(Exception )
            { }
            return result;
        }

        private static Uri GetUri(Uri baseUri, HtmlNode aHrefNode)
        {
            string url = aHrefNode.Attributes["href"].Value;
            bool isHttp = url.StartsWith("http");

            if (isHttp && Uri.IsWellFormedUriString(url, UriKind.Absolute))
                return new Uri(url);
            if (!isHttp && Uri.IsWellFormedUriString(url, UriKind.Relative))
                return new Uri(baseUri, url);

            Logger.Debug($"Skipped '{url}'. It is not a valid url.");
            return null;
        }


        //Regex _regexImpressumLink = new Regex(@"\<a\ href\=""(?<url>[^""]*)"">Impressum\<\/a\>");
        private IEnumerable<ImpressumItem> ParseForImpressumLink(HtmlDocument html, Link link)
        {
            if (LinkChecker.ImpressumChecker.IsRelevant(link))
            {
                return new ImpressumItem[] { ParseImpressum(html, link) };
            }

            return new ImpressumItem[0];

            //var result = new List<ImpressumItem>();

            //try
            //{
            //    var nodes = html.DocumentNode.SelectNodes("//a[@href]").Where(x => x.InnerText.Contains("Impressum"));

            //    foreach (var node in nodes)
            //    {
            //        var uri = GetUri(baseUri, node);
            //        if (uri == null)
            //            continue;

            //        var subHtml = await LoadHtmlAsync(uri);
            //        result.AddRange(ParseImpressum(subHtml, uri.Host));
            //    }
            //}
            //catch(Exception)
            //{ }
            //return result;
        }


        public static IEnumerable<FoundContent> Parse(HtmlDocument html, IEnumerable<IContentFinder> finders)
        {
            var finderList = finders.ToList();
            var lines = GetTextLines(html.DocumentNode);
            foreach (var line in lines)
            {
                string normalizedText = Normalize(line);
                if (string.IsNullOrWhiteSpace(normalizedText))
                    continue;

                FoundContent foundContent;
                foreach (var finder in finders)
                    if (finder.TryFind(normalizedText, out foundContent))
                        yield return foundContent;
            }
        }

        private static List<IContentFinder> finders = new List<IContentFinder>
        {
            new PhoneNumberFinder(),
            new NameFinder(),
            new TaxNumberFinder(),
            new EMailAddressFinder()
        };

        private static ImpressumItem ParseImpressum(HtmlDocument html, Link link)
        {
            var foundContentItems = Parse(html, finders).ToLookup(x => x.Type);
            string host = link.Uri.Host;
            string url = link.Uri.AbsoluteUri;

            return new ImpressumItem
            {
                Name = BestName(foundContentItems[FoundContentType.Name]),
                TelephoneNumber = BestPhone(foundContentItems[FoundContentType.PhoneNumber]),
                TaxIdentifier = BestTaxId(foundContentItems[FoundContentType.TaxIdentifiactionNumber]),
                EMailAddress = BestEMail(foundContentItems[FoundContentType.EMailAdress]),
                //Street = BestEMail(foundContentItems[FoundContentType.S]),
                Host = host,
                Url = url,
            };
        }

        private static string BestEMail(IEnumerable<FoundContent> names)
        {
            return names.FirstOrDefault()?.Content;
        }

        private static string BestName(IEnumerable<FoundContent> names)
        {
            return names.FirstOrDefault()?.Content;
        }

        private static string BestPhone(IEnumerable<FoundContent> phones)
        {
            return phones.FirstOrDefault()?.Content;
        }

        private static string BestTaxId(IEnumerable<FoundContent> taxIds)
        {
            return taxIds.FirstOrDefault()?.Content;
        }


        private static string Normalize(string line)
        {
            StringBuilder sb = new StringBuilder();
            bool wasWhitespace = false;
            foreach (var c in line)
            {
                if (!char.IsWhiteSpace(c))
                {
                    sb.Append(c);
                    wasWhitespace = false;
                }
                else if (!wasWhitespace)
                {
                    sb.Append(c);
                    wasWhitespace = true;
                }
            }
            return sb.ToString();
        }

        private static IEnumerable<string> GetTextLines(HtmlNode node)
        {
            foreach(var child in node.ChildNodes)
            {
                if (child is HtmlCommentNode)
                    continue;

                if (child.Name == "link" || child.Name=="script")
                    continue;

                if (child is HtmlTextNode)
                    yield return ((HtmlTextNode)child).Text;
                else
                    foreach (var subline in GetTextLines(child))
                        yield return subline;
            }
        }
    }
}
