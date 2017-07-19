using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Yais.Collections;

namespace Yais.Model.Search
{
    public class PriorityQueue<T>
    {
        private int _count;
        private readonly SortedDictionary<int, Queue<T>> _data = 
            new SortedDictionary<int, Queue<T>>();

        private static readonly ReaderWriterLockSlim ReaderWriterLock =
            new ReaderWriterLockSlim();

        public int Count
        {
            get
            {
                return ReaderWriterLock.ExecuteInReaderLock(()=>_count);
            }
        }

        private bool IsEmptyIntern()
        {
            return _count == 0;
        }

        public bool IsEmpty()
        {
            return ReaderWriterLock.ExecuteInReaderLock(() => IsEmptyIntern());
        }

        public T Dequeue()
        {
            return ReaderWriterLock.ExecuteInWriterLock(() =>
            {
                if (IsEmptyIntern())
                    throw new InvalidOperationException("empty queue");

                foreach (var q in _data.Values)
                {
                    if (q.Count > 0)
                    {
                        _count--;
                        return q.Dequeue();
                    }
                }
                throw new Exception("corrupt data");
            });
        }

        public bool TryDequeue(out T result)
        {
            result = default(T);

            try
            {
                ReaderWriterLock.EnterWriteLock();
                foreach (var q in _data.Values)
                {
                    if (q.Count > 0)
                    {
                        _count--;
                        result = q.Dequeue();
                        return true;
                    }
                }
            }
            finally
            {
                ReaderWriterLock.ExitWriteLock();
            }
            return false;
        }

        public T Peek()
        {
            return ReaderWriterLock.ExecuteInReaderLock(() =>
            {
                if (IsEmptyIntern())
                    throw new InvalidOperationException("empty queue");

                foreach (var q in _data.Values)
                {
                    if (q.Count > 0)
                        return q.Peek();
                }

                throw new Exception("corrupt data");
            });
        }

        public T Dequeue(int prio)
        {
            return ReaderWriterLock.ExecuteInWriterLock(() =>
            {
                _count--;
                return _data[prio].Dequeue();
            });
        }

        public void Enqueue(T item, int prio)
        {
            ReaderWriterLock.ExecuteInWriterLock(() =>
            {
                if (!_data.ContainsKey(prio))
                {
                    _data.Add(prio, new Queue<T>());
                }
                _data[prio].Enqueue(item);
                _count++;
            });
        }
    }
}