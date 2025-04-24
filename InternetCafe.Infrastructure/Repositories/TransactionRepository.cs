using InternetCafe.Domain.Entities;
using InternetCafe.Domain.Enums;
using InternetCafe.Domain.Interfaces.Repositories;
using InternetCafe.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InternetCafe.Infrastructure.Repositories
{
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<Transaction>> GetByAccountIdAsync(int accountId)
        {
            return await _dbSet
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.Creation_Timestamp)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Transaction>> GetBySessionIdAsync(int sessionId)
        {
            return await _dbSet
                .Where(t => t.SessionId == sessionId)
                .OrderByDescending(t => t.Creation_Timestamp)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Transaction>> GetByTypeAsync(TransactionType type)
        {
            return await _dbSet
                .Where(t => t.Type == type)
                .OrderByDescending(t => t.Creation_Timestamp)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalAmountByTypeAndDateRangeAsync(TransactionType type, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(t => t.Type == type &&
                           t.Creation_Timestamp >= startDate &&
                           t.Creation_Timestamp <= endDate)
                .SumAsync(t => t.Amount);
        }
    }
}