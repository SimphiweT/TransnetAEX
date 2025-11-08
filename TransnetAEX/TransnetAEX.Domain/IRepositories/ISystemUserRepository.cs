using TransnetAEX.Domain.Entities;
using TransnetAEX.Domain.IRepositories.Base;

namespace TransnetAEX.Domain.IRepositories
{
    public interface ISystemUserRepository : IRepositoryBase<SystemUser>
    {
        Task<SystemUser?> GetByEmailAsync(string email);
        Task<SystemUser?> GetUserByIdAsync(string id);
        Task<List<SystemUser>> GetAllUsersAsync();
        Task<SystemUser> UpdateUserAsync(SystemUser user);
        Task<SystemUser> CreateUserAsync(SystemUser user);
    }
}
