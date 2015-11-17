using Models;
using FastServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FastServer.Database
{
    public class RoleDao : IRoleDao
    {
        public IDbContext IContext { get; set; }

        public DbContext DbContext
        {
            get
            {
                return this.IContext as DbContext;
            }
        }

        public bool IsExist(Guid id)
        {
            return true;
        }
    }
}
