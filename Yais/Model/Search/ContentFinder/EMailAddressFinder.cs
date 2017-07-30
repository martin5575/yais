using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Yais.Model.Search.ContentFinder
{
    public class EMailAddressFinder : IContentFinder
    {
        static readonly Regex _regex = new Regex(@"([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)");

        public FoundContentType Type { get { return FoundContentType.EMailAdress; } }

        public bool TryFind(string line, out FoundContent foundContent)
        {
            var match = _regex.Match(line);
            if (!match.Success)
            {
                foundContent = null;
                return false;
            }

            foundContent = new FoundContent
            {
                Content = match.Value,
                Type = FoundContentType.EMailAdress
            };
            return true;
        }
    }
}