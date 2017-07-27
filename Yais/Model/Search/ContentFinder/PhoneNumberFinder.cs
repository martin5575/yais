using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Yais.Model.Search.ContentFinder
{
    public class PhoneNumberFinder : IContentFinder
    {
        static readonly Regex _regexPhone = new Regex(@"[\(+0]{1}[0-9 \-\(\)]{4,20}");

        public FoundContentType Type { get { return FoundContentType.PhoneNumber; } }

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
                Type = FoundContentType.PhoneNumber
            };
            return true;
        }
    }
}
