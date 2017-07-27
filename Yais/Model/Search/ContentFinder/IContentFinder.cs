using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yais.Model.Search.ContentFinder
{
    public interface IContentFinder
    {
        bool TryFind(string line, out FoundContent foundContent);
        FoundContentType Type { get; }
    }
}
