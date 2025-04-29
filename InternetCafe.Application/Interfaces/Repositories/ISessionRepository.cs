using InternetCafe.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InternetCafe.Application.Interfaces.Repositories
{
    public interface ISessionRepository : IRepository<Session>
    {
        Task<IReadOnlyList<Session>> GetActiveSessionsAsync();
        Task<IReadOnlyList<Session>> GetByUserIdAsync(int userId);
        Task<IReadOnlyList<Session>> GetByComputerIdAsync(int computerId);
        Task<Session?> GetCurrentSessionByComputerIdAsync(int computerId);
        Task EndSessionAsync(int sessionId, DateTime endTime, decimal totalCost);
    }
}