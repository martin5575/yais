using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yais.Model
{
    public class SearchResult
    {
        public SearchJob Job { get; set; }

        public List<SearchJob> SubJobs { get; set; }

        public List<ImpressumItem> Items { get; set; }
    }
}
