using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yais.Model.Search;

namespace Yais.Model
{
    public class SearchJob
    {
        public int MaxDepth { get; set; }
        public int CurrentDepth { get; set; }
        public Link Link { get; set; }
    }
}
