using Server.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Database
{
    public class DbContext : IDbContext
    {
        public bool IsDispose { get; private set; }

        public void Dispose()
        {
            this.IsDispose = true;
        }
    }
}
