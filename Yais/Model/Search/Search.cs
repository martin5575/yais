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
                Url = url,
            };
        }


        public async Task<SearchResult> SearchAsync(SearchJob job)
        {
            var uris = new List<Uri>();

            var html = await LoadHtmlAsync(job.Url);

            if (job.CurrentDepth <= job.MaxDepth)
                uris = ParseUris(html, job.Url);

            var items = await ParseForImpressumLinkAsync(html, job.Url);

            return new SearchResult
            {
                Job = job,
                SubJobs = uris.Select(x =>
                new SearchJob {
                    Url = x,
                    MaxDepth = job.MaxDepth,
                    CurrentDepth = job.CurrentDepth++
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
        private List<Uri> ParseUris(HtmlDocument html, Uri baseUri)
        {
            var result = new List<Uri>();
            foreach (HtmlNode node in html.DocumentNode.SelectNodes("//a[@href]"))
            {
                var uri = GetUri(baseUri, node);
                result.Add(uri);
            }
            return result;
        }

        private static Uri GetUri(Uri baseUri, HtmlNode aHrefNode)
        {
            string url = aHrefNode.Attributes["href"].Value;
            var uri = url.StartsWith("http") ? new Uri(url) : new Uri(baseUri, url);
            return uri;
        }

        //Regex _regexImpressumLink = new Regex(@"\<a\ href\=""(?<url>[^""]*)"">Impressum\<\/a\>");
        private async Task<List<ImpressumItem>> ParseForImpressumLinkAsync(HtmlDocument html, Uri baseUri)
        {
            if (baseUri.LocalPath.Contains("Impressum"))
            {
                return ParseImpressum(html);
            }

            var result = new List<ImpressumItem>();
            var nodes = html.DocumentNode.SelectNodes("//a[@href]").Where(x => x.InnerText.Contains("Impressum"));
            foreach (var node in nodes)
            {
                var uri = GetUri(baseUri, node);
                var subHtml = await LoadHtmlAsync(uri);
                result.AddRange(ParseImpressum(subHtml));
            }
            return result;
        }

        static readonly Regex _regexPhone = new Regex(@"[0-9]{4,10}");
        private static List<ImpressumItem> ParseImpressum(HtmlDocument html)
        {
            var data = new List<ImpressumItem>();

            var spans = html.DocumentNode.SelectNodes("//span");

            foreach (var span in spans)
            {
                foreach (Match item in _regexPhone.Matches(span.InnerText))
                {
                    data.Add(new ImpressumItem
                    {
                        TelephoneNumber = item.Value
                    });
                }
            }
            return data;
        }
    }
}
