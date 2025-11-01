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
            // Definir IsActive como true por padrão se não foi especificado
            if (tbUser.IsActive == null)
                tbUser.IsActive = true;

            // Se o usuário veio com roles só com o Id, precisamos "attachar"
            if (tbUser.Roles != null)
            {
                var attachedRoles = new List<Role>();

                foreach (var role in tbUser.Roles)
                {
                    if (role.Id != 0)
                    {
                        // cria um stub com o Id e anexa ao contexto
                        var roleStub = new Role { Id = role.Id };
                        _dbContext.Roles.Attach(roleStub);
                        attachedRoles.Add(roleStub);
                    }
                }

                // substitui as roles pelo que está devidamente trackeado
                tbUser.Roles = attachedRoles;
            }

            await _dbContext.Users.AddAsync(tbUser);
            await _dbContext.SaveChangesAsync();
        }


        public async Task<User?> FindUserByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;
                
            return await _dbContext.Users
                .Include(u => u.Roles) // Incluir os roles na consulta
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && (u.IsActive == true || u.IsActive == null));
        }

    }
}
