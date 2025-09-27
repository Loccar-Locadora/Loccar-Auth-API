using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoccarInfra.ORM.model;

namespace LoccarInfra.Interfaces
{
    public interface IAuthRepository
    {
        Task RegisterUser(User tbUser);
        Task<User> FindUserByEmail(string email);
    }
}
