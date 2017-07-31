using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yais.Model.Search.ContentFinder
{
    [DebuggerDisplay("{Type} {Content}")]
    public class FoundContent
    {
        public FoundContentType Type { get; set; }
        public string Content { get; set; }
    }
}
