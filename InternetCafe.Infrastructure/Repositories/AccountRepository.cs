using InternetCafe.Domain.Entities;
using InternetCafe.Application.Interfaces.Repositories;
using InternetCafe.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace InternetCafe.Infrastructure.Repositories
{
    public class AccountRepository : GenericRepository<Account>, IAccountRepository
    {
        public AccountRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Account?> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(a => a.UserId == userId);
        }

        public async Task<Account?> GetWithTransactionsAsync(int accountId)
        {
            return await _dbSet
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.Id == accountId);
        }

        public async Task UpdateBalanceAsync(int accountId, decimal amount)
        {
            var account = await _dbSet.FindAsync(accountId);
            if (account != null)
            {
                account.Balance += amount;
                account.LastDepositDate = (amount > 0) ? DateTime.Now : account.LastDepositDate;
                account.LastUsageDate = (amount < 0) ? DateTime.Now : account.LastUsageDate;

                _dbContext.Entry(account).State = EntityState.Modified;
            }
        }
    }
}