using InternetCafe.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InternetCafe.Domain.Interfaces.Services
{
    public interface ISessionService
    {
        Task<Session> StartSessionAsync(int userId, int computerId);
        Task<Session> EndSessionAsync(int sessionId);
        Task<Session> GetActiveSessionByComputerIdAsync(int computerId);
        Task<List<Session>> GetActiveSessionsAsync();
        Task<List<Session>> GetSessionsByUserIdAsync(int userId);
        Task<List<Session>> GetSessionsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<decimal> CalculateSessionCostAsync(int sessionId);
        Task<decimal> CalculateSessionCostAsync(TimeSpan duration, decimal hourlyRate);
        Task<TimeSpan> GetRemainingTimeAsync(int userId, int computerId);
        Task<bool> HasActiveSessionAsync(int userId);
        Task TerminateSessionAsync(int sessionId, string reason);
    }
}