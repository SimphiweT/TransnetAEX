using Microsoft.AspNetCore.Identity;
using TransnetAEX.Domain.Entities;
using TransnetAEX.Domain.IRepositories;
using TransnetAEX.Infrastructure.Data;
using TransnetAEX.Infrastructure.EF_Repositories.Base;

namespace TransnetAEX.Infrastructure.EF_Repositories
{
    public class SystemUserRolesRepository : RepositoryBase<SystemUserRoles>, ISystemUserRolesRepository
    {
        private readonly RoleManager<SystemUserRoles> _roleManager;

        public SystemUserRolesRepository(AppDbContext dbContext, RoleManager<SystemUserRoles> roleManager) : base(dbContext)
        {
            _roleManager = roleManager;
        }
    }
}
