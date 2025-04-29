using InternetCafe.Domain.Entities;
using InternetCafe.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InternetCafe.Application.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetUserWithAccountAsync(int userId);
        Task<IReadOnlyList<User>> GetUsersByRoleAsync(UserRole role);
    }
}