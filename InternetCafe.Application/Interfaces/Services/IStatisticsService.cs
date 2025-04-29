using System;
using System.Threading.Tasks;

namespace InternetCafe.Application.Interfaces.Services
{
    public interface IStatisticsService
    {
        Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
        Task<int> GetActiveUsersCountAsync();
        Task<int> GetActiveSessionsCountAsync();
        Task<int> GetComputersInUseCountAsync();
        Task<int> GetAvailableComputersCountAsync();
        Task<decimal> GetAverageSessionDurationAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetAverageRevenuePerUserAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetAverageRevenuePerComputerAsync(DateTime startDate, DateTime endDate);
        Task<(DateTime TimeOfDay, int Count)[]> GetPeakUsageHoursAsync(DateTime startDate, DateTime endDate);
        Task<(string UserName, TimeSpan TotalTime)[]> GetTopUsersAsync(DateTime startDate, DateTime endDate, int count = 10);
    }
}