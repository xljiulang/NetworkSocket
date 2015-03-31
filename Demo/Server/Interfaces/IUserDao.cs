using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Interfaces
{
    public interface IUserDao : IDao
    {
        bool IsExist(User user);
    }
}
