using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yais.Model.Search
{
    public class LinkChecker
    {

        public static readonly LinkChecker ImpressumChecker = new LinkChecker("Impressum");


        List<string> _namesToLower;
        public LinkChecker(params string[] names)
        { 
            _namesToLower = names.Select(x => x.ToLowerInvariant()).ToList();
        }

        public bool IsRelevant(Link link)
        {
            var uriLower = link.Uri.AbsoluteUri.ToLowerInvariant();
            if (_namesToLower.Any(x => uriLower.Contains(x)))
                return true;

            var nameLower = link.Name.ToLowerInvariant();
            return _namesToLower.Any(x => uriLower.Contains(x));
        }
    }
}
