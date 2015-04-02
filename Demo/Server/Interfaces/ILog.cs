using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Interfaces
{
    public interface ILog : IDisposable
    {
        bool Write(string log);
    }
}
