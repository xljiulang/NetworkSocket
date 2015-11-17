using FastServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FastServer.Database
{
    public class Loger : ILog
    {
        public bool IsDisposed { get; private set; }

        public bool Write(string log)
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException("Loger");
            }
            Console.WriteLine(log);
            return true;
        }

        public void Dispose()
        {
            this.IsDisposed = true;
        }
    }
}
