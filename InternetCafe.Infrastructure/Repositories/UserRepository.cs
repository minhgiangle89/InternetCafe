using InternetCafe.Domain.Entities;
using InternetCafe.Domain.Enums;
using InternetCafe.Application.Interfaces.Repositories;
using InternetCafe.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InternetCafe.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserWithAccountAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.Account)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<IReadOnlyList<User>> GetUsersByRoleAsync(UserRole role)
        {
            return await _dbSet
                .Where(u => u.Role == (int)role)
                .ToListAsync();
        }
    }
}