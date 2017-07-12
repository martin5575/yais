using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Yais.Model
{
    public class Search
    {
        static HttpClient _client = new HttpClient();

        //public async Task<IEnumerable<SearchJob>> StartAsync(string searchWords, int depth)
        //{
        //    var job = GetSearchJob(searchWords, depth);
        //    var searchResult = await SearchAsync(job);

        //    var subJobs = searchResult.SubUris.Select(x => new SearchJob
        //    {
        //        Url = x,
        //        MaxDepth = job.MaxDepth,
        //        CurrentDepth = job.CurrentDepth++
        //    });

        //    return;
        //}

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
            var items = new List<ImpressumItem>();

            var content = await _client.GetStringAsync(job.Url);

            if (job.CurrentDepth <= job.MaxDepth)
                uris = ParseUris(content, job.Url);

            items = await ParseForImpressumLinkAsync(content, job.Url);

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

        Regex _regex = new Regex(@"\<a\ href\=""(?<url>[^""]*)"">[^\<]*\<\/a\>");
        private List<Uri> ParseUris(string content, Uri baseUri)
        {
            var result = new List<Uri>();
            foreach (Match item in _regex.Matches(content))
            {
                string url = item.Groups["url"].Value;
                var uri = url.StartsWith("http") ? new Uri(url) : new Uri(baseUri, url);
                result.Add(uri);
            }
            return result;
        }

        Regex _regexImpressumLink = new Regex(@"\<a\ href\=""(?<url>[^""]*)"">Impressum\<\/a\>");
        private async Task<List<ImpressumItem>> ParseForImpressumLinkAsync(string content, Uri baseUri)
        {
            string textToParse;
            if (_regexImpressumLink.IsMatch(content))
            {
                string relativeUri = _regexImpressumLink.Match(content).Groups["url"].Value;
                var uri = new Uri(baseUri, relativeUri);
                textToParse = await _client.GetStringAsync(uri);
            }
            else
                textToParse = content;

            return ParseImpressum(textToParse);
        }

        static Regex _regexPhone = new Regex(@"[0-9]{4,10}");
        private static List<ImpressumItem> ParseImpressum(string text)
        {
            var data = new List<ImpressumItem>();
            foreach (Match item in _regexPhone.Matches(text))
            {
                data.Add(new ImpressumItem
                {
                    TelephoneNumber = item.Value
                });
            }
            return data;
        }
    }
}
