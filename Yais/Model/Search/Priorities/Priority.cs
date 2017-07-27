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
            var url = job.Url;
            if (url.AbsoluteUri.ToLowerInvariant().Contains("Impressum"))
                return 1;

            var host = url.Host;
            var depth = job.CurrentDepth;
            var maxDepth = job.MaxDepth;

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
