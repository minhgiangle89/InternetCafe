using InternetCafe.Application.DTOs.Statistics;
using System;
using System.Threading.Tasks;

namespace InternetCafe.Application.Interfaces.Services
{
    public interface IStatisticsService
    {
        Task<StatisticsSummaryDTO> GetStatisticsSummaryAsync();
        Task<RevenueSummaryDTO> GetRevenueSummaryAsync(DateTime startDate, DateTime endDate);
        Task<UsageStatisticsDTO> GetUsageStatisticsAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<TopUserDTO>> GetTopUsersAsync(DateTime startDate, DateTime endDate, int count = 10);
        Task<IEnumerable<DailyRevenueDTO>> GetDailyRevenueAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
        Task<int> GetActiveUsersCountAsync();
        Task<int> GetActiveSessionsCountAsync();
        Task<int> GetComputersInUseCountAsync();
        Task<int> GetAvailableComputersCountAsync();
    }
}