using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoccarInfra.Interfaces;
using LoccarInfra.ORM.model;
using Microsoft.EntityFrameworkCore;

namespace LoccarInfra.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        readonly DataBaseContext _dbContext;
        public AuthRepository(DataBaseContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task RegisterUser(User tbUser)
        {
            await _dbContext.Users.AddAsync(tbUser);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<User> FindUserByEmail(string email)
        {
            return await _dbContext.Users
                                   .Where(n => n.Email.Equals(email))
                                   .FirstOrDefaultAsync();
        }

    }
}
