using InternetCafe.Application.DTOs.Session;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InternetCafe.Application.Interfaces.Services
{
    public interface ISessionService
    {
        Task<SessionDTO> StartSessionAsync(StartSessionDTO startSessionDTO);
        Task<SessionDTO> EndSessionAsync(EndSessionDTO endSessionDTO);
        Task<SessionDTO> GetActiveSessionByComputerIdAsync(int computerId);
        Task<IEnumerable<SessionDTO>> GetActiveSessionsAsync();
        Task<IEnumerable<SessionDTO>> GetSessionsByUserIdAsync(int userId);
        Task<IEnumerable<SessionDTO>> GetSessionsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<decimal> CalculateSessionCostAsync(int sessionId);
        Task<decimal> CalculateSessionCostAsync(TimeSpan duration, decimal hourlyRate);
        Task<TimeSpan> GetRemainingTimeAsync(int userId, int computerId);
        Task<bool> HasActiveSessionAsync(int userId);
        Task<SessionDTO> TerminateSessionAsync(int sessionId, string reason);
        Task<SessionDetailsDTO> GetSessionDetailsAsync(int sessionId);
    }
}