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
    public class SessionRepository : GenericRepository<Session>, ISessionRepository
    {
        public SessionRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<Session>> GetActiveSessionsAsync()
        {
            return await _dbSet
                .Where(s => s.Status == SessionStatus.Active)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Session>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Session>> GetByComputerIdAsync(int computerId)
        {
            return await _dbSet
                .Where(s => s.ComputerId == computerId)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<Session?> GetCurrentSessionByComputerIdAsync(int computerId)
        {
            return await _dbSet
                .Where(s => s.ComputerId == computerId && s.Status == SessionStatus.Active)
                .FirstOrDefaultAsync();
        }

        public async Task EndSessionAsync(int sessionId, DateTime endTime, decimal totalCost)
        {
            var session = await _dbSet.FindAsync(sessionId);
            if (session != null)
            {
                session.EndTime = endTime;
                session.Duration = endTime - session.StartTime;
                session.TotalCost = totalCost;
                session.Status = SessionStatus.Completed;

                _dbContext.Entry(session).State = EntityState.Modified;
            }
        }
    }
}