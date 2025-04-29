using InternetCafe.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InternetCafe.Application.Interfaces.Repositories
{
    public interface IAccountRepository : IRepository<Account>
    {
        Task<Account?> GetByUserIdAsync(int userId);
        Task<Account?> GetWithTransactionsAsync(int accountId);
        Task UpdateBalanceAsync(int accountId, decimal amount);
    }
}