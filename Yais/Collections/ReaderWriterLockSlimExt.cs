using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Yais.Collections
{
    public static class ReaderWriterLockSlimExt
    {
        public static T ExecuteInReaderLock<T>(this ReaderWriterLockSlim rwLock, Func<T> func)
        {
            try
            {
                rwLock.EnterReadLock();
                return func();
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }

        public static void ExecuteInReaderLock(this ReaderWriterLockSlim rwLock, Action action)
        {
            try
            {
                rwLock.EnterReadLock();
                action();
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }

        public static T ExecuteInWriterLock<T>(this ReaderWriterLockSlim rwLock, Func<T> func)
        {
            try
            {
                rwLock.EnterWriteLock();
                return func();
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }


        public static void ExecuteInWriterLock(this ReaderWriterLockSlim rwLock, Action action)
        {
            try
            {
                rwLock.EnterWriteLock();
                action();
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }


        public static T ExecuteInUpgradeableReaderLock<T>(this ReaderWriterLockSlim rwLock, Func<T> func)
        {
            try
            {
                rwLock.EnterUpgradeableReadLock();
                return func();
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }
        }
        public static void ExecuteInUpgradeableReaderLock(this ReaderWriterLockSlim rwLock, Action action)
        {
            try
            {
                rwLock.EnterUpgradeableReadLock();
                action();
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }
        }

    }
}