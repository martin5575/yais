using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Yais.Model.Search.ContentFinder
{
    public class CityFinder : IContentFinder
    {
        static readonly Regex _regex = new Regex(@"(D\-)?([0-9]{5})( )+(?<City>[A-ZÄÖÜ][a-zäöüß\/]+(( |\-)?[a-zäüöA-ZÄÖÜ][a-zäöüß\.]+){0,3})");

        public FoundContentType Type { get { return FoundContentType.City; } }

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
                Content = match.Groups["City"].Value,
                Type = FoundContentType.City
            };
            return true;
        }
    }
}
