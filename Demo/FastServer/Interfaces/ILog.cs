using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FastServer.Interfaces
{
    public interface ILog : IDisposable
    {
        bool Write(string log);
    }
}
