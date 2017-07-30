using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Yais.Collections;

namespace Yais.Model.Search.Priorities
{
    public class Priority
    {
        HashSet<string> _visitedHosts = new HashSet<string>();
        ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();



        public int GetBestPrio()
        {
            return 1;
        }

        public int GetPrio(SearchJob job)
        {
            if (LinkChecker.ImpressumChecker.IsRelevant(job.Link))
                return 1; // top prio

            var url = job.Link.Uri;

            var host = url.Host;
            var depth = job.CurrentDepth;
            var maxDepth = job.MaxDepth;
            var isDeDomain = host.EndsWith(".de");

            if (!isDeDomain)
                return 500000;


            double factor = (maxDepth + 1.0) / (depth + 1.0);
            return _readerWriterLock.ExecuteInUpgradeableReaderLock(() =>
            {
                if (_visitedHosts.Contains(host))
                    return (int)Math.Ceiling(100 * factor);
                else
                {
                    return _readerWriterLock.ExecuteInWriterLock(() =>
                    {
                        _visitedHosts.Add(host);
                        return (int)Math.Ceiling(10 * factor);
                    });
                }
            });

        }
    }
}
