using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yais.Data;

namespace Yais.Model.Search.ContentFinder
{
    public class NameFinder : IContentFinder
    {
        static HashSet<string> _allowedGivenNames;

        static readonly Regex _regexName = new Regex(
            @"(?<GivenName>[A-ZÄÖÜ][a-zäöüß]+)(\-[A-ZÄÖÜ][a-zäöüß]+)?( ([a-zäöüß]* ){0,5}[A-ZÄÖÜ][a-zäöüß]+(\-[A-ZÄÖÜ][a-zäöüß]+)?){1,4}");


        static NameFinder()
        {
            string data = ResourceReader.ReadResource("GivenNames.txt");

            _allowedGivenNames = new HashSet<string>();
            foreach (var line in data.Split('\r'))
                _allowedGivenNames.Add(line.Trim(' ','\r','\n'));
        }

        public FoundContentType Type { get { return FoundContentType.Name; } }

        public bool TryFind(string line, out FoundContent foundContent)
        {
            foundContent = null;

            var match = _regexName.Match(line);
            if (!match.Success)
                return false;

            string givenName = match.Groups["GivenName"].Value;
            if (!_allowedGivenNames.Contains(givenName))
                return false;

            foundContent = new FoundContent
            {
                Content = match.Value,
                Type = FoundContentType.Name
            };
            return true;
        }
    }
}
