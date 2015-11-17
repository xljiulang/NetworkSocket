using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FastServer.Interfaces
{
    public interface IUserDao : IDao
    {
        bool IsExist(User user);
    }
}
