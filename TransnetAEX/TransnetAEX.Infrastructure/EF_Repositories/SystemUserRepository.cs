using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TransnetAEX.Domain.Entities;
using TransnetAEX.Domain.IRepositories;
using TransnetAEX.Infrastructure.Data;
using TransnetAEX.Infrastructure.EF_Repositories.Base;

namespace TransnetAEX.Infrastructure.EF_Repositories
{
    public class SystemUserRepository : RepositoryBase<SystemUser>, ISystemUserRepository
    {
        private readonly UserManager<SystemUser> userManager;

        public SystemUserRepository(AppDbContext dbContext, UserManager<SystemUser> _userManager) : base(dbContext)
        {
            userManager = _userManager;
        }

        public async Task<SystemUser> CreateUserAsync(SystemUser user)
        {
            // Note: Password and roles handled in controller/service, not here
            var result = await userManager.CreateAsync(user);
            if (!result.Succeeded)
                throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));
            return user;
        }

        public async Task<List<SystemUser>> GetAllUsersAsync()
        {
            return await userManager.Users.ToListAsync();
        }

        public async Task<SystemUser?> GetByEmailAsync(string email)
        {
            return await appDbContext.SystemUsers.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<SystemUser?> GetUserByIdAsync(string id)
        {
            return await userManager.FindByIdAsync(id);
        }

        public async Task<SystemUser> UpdateUserAsync(SystemUser user)
        {
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));
            return user;
        }
    }
}
