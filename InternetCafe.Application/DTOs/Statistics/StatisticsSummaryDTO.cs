using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetCafe.Application.DTOs.Statistics
{
    public class StatisticsSummaryDTO
    {
        public decimal TotalRevenue { get; set; }
        public int ActiveUsersCount { get; set; }
        public int ActiveSessionsCount { get; set; }
        public int ComputersInUseCount { get; set; }
        public int AvailableComputersCount { get; set; }
    }

    public class RevenueSummaryDTO
    {
        public decimal TotalRevenue { get; set; }
        public decimal AverageRevenuePerUser { get; set; }
        public decimal AverageRevenuePerComputer { get; set; }
        public List<DailyRevenueDTO> DailyRevenue { get; set; } = new List<DailyRevenueDTO>();
    }

    public class DailyRevenueDTO
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }

    public class UsageStatisticsDTO
    {
        public decimal AverageSessionDuration { get; set; }
        public List<HourlyUsageDTO> PeakUsageHours { get; set; } = new List<HourlyUsageDTO>();
        public List<TopUserDTO> TopUsers { get; set; } = new List<TopUserDTO>();
    }

    public class HourlyUsageDTO
    {
        public int Hour { get; set; }
        public int Count { get; set; }
    }

    public class TopUserDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public TimeSpan TotalTime { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
