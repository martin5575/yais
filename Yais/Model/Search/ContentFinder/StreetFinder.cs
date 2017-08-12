using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Yais.Model.Search.ContentFinder
{
    public class StreetFinder
    {
        static readonly Regex _regex = new Regex(@"(?<Street>[A-ZÄÖÜ][a-zäöüß\/]+(( |\-)?[a-zäüöA-ZÄÖÜ][a-zäöüß\.]+){0,3}) [1-9][0-9]{0,3}");

        public FoundContentType Type { get { return FoundContentType.Street; } }

        public bool TryFind(string line, out FoundContent foundContent)
        {
            foundContent = null;

            var match = _regex.Match(line);
            if (!match.Success)
                return false;

            var street = match.Groups["Street"].Value;
            if (!IsStreet(street))
                return false;

            foundContent = new FoundContent
            {
                Content = street,
                Type = FoundContentType.Street
            };
            return true;
        }

        private bool IsStreet(string street)
        {
            var lower = street.ToLowerInvariant();
            if (lower.Contains("straße"))
                return true;
            if (lower.Contains("str."))
                return true;
            if (lower.Contains("platz"))
                return true;
            if (lower.Contains("gasse"))
                return true;
            if (lower.Contains("gässchen"))
                return true;
            if (lower.Contains("pfad"))
                return true;
            if (lower.Contains("weg"))
                return true;
            if (lower.Contains("ring"))
                return true;

            return false;
        }
    }
}
