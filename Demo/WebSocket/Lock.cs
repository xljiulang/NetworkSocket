using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WebSocket
{
    public class Lock : IDisposable
    {
        private ReaderWriterLockSlim lockSlim;

        public Lock()
        {
            this.lockSlim = new ReaderWriterLockSlim();
        }

        public IDisposable Read()
        {
            return new LockFor(LockType.Read, this.lockSlim);
        }

        public IDisposable Write()
        {
            return new LockFor(LockType.Write, this.lockSlim);
        }

        public IDisposable Upgrad()
        {
            return new LockFor(LockType.Upgrad, this.lockSlim);
        }      


        public void Dispose()
        {
            this.lockSlim.Dispose();
        }

        private enum LockType
        {
            Read,
            Write,
            Upgrad
        }

        private class LockFor : IDisposable
        {
            private LockType lockType;

            private ReaderWriterLockSlim lockSlim;

            public LockFor(LockType type, ReaderWriterLockSlim slim)
            {
                this.lockType = type;
                this.lockSlim = slim;

                switch (lockType)
                {
                    case LockType.Read:
                        slim.EnterReadLock();
                        break;

                    case LockType.Write:
                        slim.EnterWriteLock();
                        break;

                    case LockType.Upgrad:
                        slim.EnterUpgradeableReadLock();
                        break;
                }
            }

            public void Dispose()
            {
                switch (this.lockType)
                {
                    case LockType.Read:
                        this.lockSlim.ExitReadLock();
                        break;
                    case LockType.Write:
                        this.lockSlim.ExitWriteLock();
                        break;
                    case LockType.Upgrad:
                        this.lockSlim.ExitUpgradeableReadLock();
                        break;
                }
            }
        }
    }
}
