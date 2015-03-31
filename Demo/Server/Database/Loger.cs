using Server.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Database
{
    public class Loger : ILog
    {
        public bool Log(string txt)
        {
            return true;
        }

        public void Dispose()
        {
        }
    }
}
