﻿using System;
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

            int nextDepth = job.CurrentDepth++;
            int maxDepth = job.MaxDepth;
            return new SearchResult
            {
                Job = job,
                SubJobs = uris.Select(x =>
                new SearchJob {
                    Url = x,
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
        private List<Uri> ParseUris(HtmlDocument html, Uri baseUri)
        {
            var result = new List<Uri>();
            try
            {
                foreach (HtmlNode node in html.DocumentNode.SelectNodes("//a[@href]"))
                {
                    var uri = GetUri(baseUri, node);
                    if (uri!=null)
                        result.Add(uri);
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
        private async Task<List<ImpressumItem>> ParseForImpressumLinkAsync(HtmlDocument html, Uri baseUri)
        {
            if (baseUri.LocalPath.Contains("Impressum"))
            {
                return ParseImpressum(html, baseUri.Host);
            }

            var result = new List<ImpressumItem>();

            try
            {
                var nodes = html.DocumentNode.SelectNodes("//a[@href]").Where(x => x.InnerText.Contains("Impressum"));

                foreach (var node in nodes)
                {
                    var uri = GetUri(baseUri, node);
                    if (uri == null)
                        continue;

                    var subHtml = await LoadHtmlAsync(uri);
                    result.AddRange(ParseImpressum(subHtml, uri.Host));
                }
            }
            catch(Exception)
            { }
            return result;
        }

        static readonly Regex _regexPhone = new Regex(@"[0-9 +()]{4,20}");
        private static List<ImpressumItem> ParseImpressum(HtmlDocument html, string host)
        {
            var data = new List<ImpressumItem>();

            var lines = GetTextLines(html.DocumentNode);

            foreach (var line in lines)
            {
                foreach (Match item in _regexPhone.Matches(line))
                {
                    data.Add(new ImpressumItem
                    {
                        TelephoneNumber = item.Value,
                        Host = host,
                    });
                }
            }
            return data;
        }

        private static IEnumerable<string> GetTextLines(HtmlNode node)
        {
            foreach(var child in node.ChildNodes)
            {
                if (child is HtmlTextNode)
                    yield return ((HtmlTextNode)child).Text;
                else
                    foreach (var subline in GetTextLines(child))
                        yield return subline;
            }
        }
    }
}
