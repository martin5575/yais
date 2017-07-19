using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yais.Model
{
    public class SearchJob
    {
        public int MaxDepth { get; set; }
        public int CurrentDepth { get; set; }
        public Uri Url { get; set; }
    }
}
