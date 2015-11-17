using Models;
using FastServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FastServer.Database
{
    public class UserDao : IUserDao
    {
        public IDbContext IContext { get; set; }

        public DbContext DbContext
        {
            get
            {
                return this.IContext as DbContext;
            }
        }

        public bool IsExist(User user)
        {
            return user != null && user.Account == "admin" && user.Password == "123456";
        }
    }
}
