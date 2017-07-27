using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Yais.Model.Search.ContentFinder
{
    public class TaxNumberFinder : IContentFinder
    {
        static readonly Regex _regexPhone = new Regex(@"DE[0-9]{9}");

        public FoundContentType Type { get { return FoundContentType.TaxIdentifiactionNumber; } }

        public bool TryFind(string line, out FoundContent foundContent)
        {
            var match = _regexPhone.Match(line);
            if (!match.Success)
            {
                foundContent = null;
                return false;
            }

            foundContent = new FoundContent
            {
                Content = match.Value,
                Type = FoundContentType.TaxIdentifiactionNumber
            };
            return true;
        }
    }
}
