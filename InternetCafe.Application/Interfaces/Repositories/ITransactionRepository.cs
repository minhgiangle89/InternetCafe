using InternetCafe.Domain.Entities;
using InternetCafe.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InternetCafe.Application.Interfaces.Repositories
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<IReadOnlyList<Transaction>> GetByAccountIdAsync(int accountId);
        Task<IReadOnlyList<Transaction>> GetBySessionIdAsync(int sessionId);
        Task<IReadOnlyList<Transaction>> GetByTypeAsync(TransactionType type);
        Task<decimal> GetTotalAmountByTypeAndDateRangeAsync(TransactionType type, DateTime startDate, DateTime endDate);
    }
}