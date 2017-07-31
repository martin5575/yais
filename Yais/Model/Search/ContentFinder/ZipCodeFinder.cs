using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Yais.Model.Search.ContentFinder
{
    public class ZipCodeFinder : IContentFinder
    {
        static readonly Regex _regex = new Regex(@"(D\-)?(?<PLZ>[0-9]{5})( )+[A-ZÄÖÜ][a-zäöüß]+");

        public FoundContentType Type { get { return FoundContentType.ZipCode; } }

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
                Content = match.Groups["PLZ"].Value,
                Type = FoundContentType.ZipCode
            };
            return true;
        }
    }
}
