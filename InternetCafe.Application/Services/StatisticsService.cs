using InternetCafe.Application.DTOs.Statistics;
using InternetCafe.Application.Interfaces;
using InternetCafe.Application.Interfaces.Services;
using InternetCafe.Domain.Enums;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InternetCafe.Application.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<StatisticsService> _logger;

        public StatisticsService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<StatisticsService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<StatisticsSummaryDTO> GetStatisticsSummaryAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var startOfDay = now.Date;
                var endOfDay = startOfDay.AddDays(1).AddSeconds(-1);

                // Daily revenue
                var dailyRevenue = await GetTotalRevenueAsync(startOfDay, endOfDay);

                // Active users
                var activeUsersCount = await GetActiveUsersCountAsync();

                // Active sessions
                var activeSessionsCount = await GetActiveSessionsCountAsync();

                // Computers in use
                var computersInUseCount = await GetComputersInUseCountAsync();

                // Available computers
                var availableComputersCount = await GetAvailableComputersCountAsync();

                return new StatisticsSummaryDTO
                {
                    TotalRevenue = dailyRevenue,
                    ActiveUsersCount = activeUsersCount,
                    ActiveSessionsCount = activeSessionsCount,
                    ComputersInUseCount = computersInUseCount,
                    AvailableComputersCount = availableComputersCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving statistics summary");
                throw;
            }
        }

        public async Task<RevenueSummaryDTO> GetRevenueSummaryAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Validate date range
                if (startDate > endDate)
                {
                    throw new ArgumentException("Start date must be before end date");
                }

                // Get daily revenue
                var dailyRevenue = await GetDailyRevenueAsync(startDate, endDate);

                // Calculate total revenue
                var totalRevenue = dailyRevenue.Sum(d => d.Amount);

                // Calculate average revenue per user
                var activeUsers = await _unitOfWork.Users.GetAllAsync();
                var activeUsersCount = activeUsers.Count;
                var averageRevenuePerUser = activeUsersCount > 0 ? totalRevenue / activeUsersCount : 0;

                // Calculate average revenue per computer
                var computers = await _unitOfWork.Computers.GetAllAsync();
                var computersCount = computers.Count;
                var averageRevenuePerComputer = computersCount > 0 ? totalRevenue / computersCount : 0;

                return new RevenueSummaryDTO
                {
                    TotalRevenue = totalRevenue,
                    AverageRevenuePerUser = averageRevenuePerUser,
                    AverageRevenuePerComputer = averageRevenuePerComputer,
                    DailyRevenue = dailyRevenue.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving revenue summary for period {StartDate} to {EndDate}",
                    startDate, endDate);
                throw;
            }
        }

        public async Task<UsageStatisticsDTO> GetUsageStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Validate date range
                if (startDate > endDate)
                {
                    throw new ArgumentException("Start date must be before end date");
                }

                // Get all sessions within the date range
                var sessions = await _unitOfWork.Sessions.FindAsync(s =>
                    s.StartTime >= startDate &&
                    (s.EndTime == null || s.EndTime <= endDate) &&
                    s.Status != SessionStatus.Terminated);

                if (!sessions.Any())
                {
                    return new UsageStatisticsDTO
                    {
                        AverageSessionDuration = 0,
                        PeakUsageHours = new List<HourlyUsageDTO>(),
                        TopUsers = new List<TopUserDTO>()
                    };
                }

                // Calculate average session duration
                var completedSessions = sessions.Where(s => s.EndTime.HasValue).ToList();
                TimeSpan averageDuration = TimeSpan.Zero;

                if (completedSessions.Any())
                {
                    var totalDuration = completedSessions.Aggregate(TimeSpan.Zero, (total, session) => total + session.Duration);
                    averageDuration = TimeSpan.FromTicks(totalDuration.Ticks / completedSessions.Count);
                }

                // Calculate peak usage hours
                var hourlyUsage = new Dictionary<int, int>();
                foreach (var session in sessions)
                {
                    var startHour = session.StartTime.Hour;
                    var endHour = session.EndTime.HasValue ? session.EndTime.Value.Hour : DateTime.UtcNow.Hour;

                    // Add each hour of the session to the count
                    for (int hour = startHour; hour <= endHour; hour++)
                    {
                        int normalizedHour = hour % 24;
                        if (hourlyUsage.ContainsKey(normalizedHour))
                        {
                            hourlyUsage[normalizedHour]++;
                        }
                        else
                        {
                            hourlyUsage[normalizedHour] = 1;
                        }
                    }
                }

                var peakUsageHours = hourlyUsage
                    .Select(kv => new HourlyUsageDTO { Hour = kv.Key, Count = kv.Value })
                    .OrderByDescending(h => h.Count)
                    .Take(5)
                    .ToList();

                // Calculate top users
                var userSessionGroups = sessions
                    .GroupBy(s => s.UserId)
                    .OrderByDescending(g => g.Sum(s => s.TotalCost))
                    .Take(10)
                    .ToList();

                var topUsers = new List<TopUserDTO>();
                foreach (var group in userSessionGroups)
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(group.Key);
                    if (user != null)
                    {
                        var userSessions = group.ToList();
                        var totalDuration = userSessions
                            .Where(s => s.EndTime.HasValue)
                            .Aggregate(TimeSpan.Zero, (total, session) => total + session.Duration);

                        var totalSpent = userSessions.Sum(s => s.TotalCost);

                        topUsers.Add(new TopUserDTO
                        {
                            UserId = user.Id,
                            UserName = user.Username,
                            TotalTime = totalDuration,
                            TotalSpent = totalSpent
                        });
                    }
                }

                return new UsageStatisticsDTO
                {
                    AverageSessionDuration = (decimal)averageDuration.TotalMinutes,
                    PeakUsageHours = peakUsageHours,
                    TopUsers = topUsers
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving usage statistics for period {StartDate} to {EndDate}",
                    startDate, endDate);
                throw;
            }
        }

        public async Task<IEnumerable<TopUserDTO>> GetTopUsersAsync(DateTime startDate, DateTime endDate, int count = 10)
        {
            try
            {
                // Validate date range
                if (startDate > endDate)
                {
                    throw new ArgumentException("Start date must be before end date");
                }

                // Get all sessions within the date range
                var sessions = await _unitOfWork.Sessions.FindAsync(s =>
                    s.StartTime >= startDate &&
                    (s.EndTime == null || s.EndTime <= endDate) &&
                    s.Status != SessionStatus.Terminated);

                if (!sessions.Any())
                {
                    return new List<TopUserDTO>();
                }

                // Group sessions by user
                var userSessionGroups = sessions
                    .GroupBy(s => s.UserId)
                    .OrderByDescending(g => g.Sum(s => s.TotalCost))
                    .Take(count)
                    .ToList();

                var topUsers = new List<TopUserDTO>();
                foreach (var group in userSessionGroups)
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(group.Key);
                    if (user != null)
                    {
                        var userSessions = group.ToList();
                        var totalDuration = userSessions
                            .Where(s => s.EndTime.HasValue)
                            .Aggregate(TimeSpan.Zero, (total, session) => total + session.Duration);

                        var totalSpent = userSessions.Sum(s => s.TotalCost);

                        topUsers.Add(new TopUserDTO
                        {
                            UserId = user.Id,
                            UserName = user.Username,
                            TotalTime = totalDuration,
                            TotalSpent = totalSpent
                        });
                    }
                }

                return topUsers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top users for period {StartDate} to {EndDate}",
                    startDate, endDate);
                throw;
            }
        }

        public async Task<IEnumerable<DailyRevenueDTO>> GetDailyRevenueAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Validate date range
                if (startDate > endDate)
                {
                    throw new ArgumentException("Start date must be before end date");
                }

                var result = new List<DailyRevenueDTO>();

                // Iterate through each day in the range
                for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    var dayStart = date;
                    var dayEnd = date.AddDays(1).AddSeconds(-1);

                    // Get transactions for the day
                    var revenue = await _unitOfWork.Transactions.GetTotalAmountByTypeAndDateRangeAsync(
                        TransactionType.ComputerUsage, dayStart, dayEnd);

                    // Add to result
                    result.Add(new DailyRevenueDTO
                    {
                        Date = date,
                        Amount = Math.Abs(revenue) // Revenue is stored as negative in transactions
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving daily revenue for period {StartDate} to {EndDate}",
                    startDate, endDate);
                throw;
            }
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Validate date range
                if (startDate > endDate)
                {
                    throw new ArgumentException("Start date must be before end date");
                }

                // Get total computer usage charges
                var usageRevenue = await _unitOfWork.Transactions.GetTotalAmountByTypeAndDateRangeAsync(
                    TransactionType.ComputerUsage, startDate, endDate);

                // Revenue is stored as negative in transactions
                return Math.Abs(usageRevenue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total revenue for period {StartDate} to {EndDate}",
                    startDate, endDate);
                throw;
            }
        }

        public async Task<int> GetActiveUsersCountAsync()
        {
            try
            {
                // Count users with active sessions
                var sessions = await _unitOfWork.Sessions.GetActiveSessionsAsync();
                var uniqueUserIds = sessions.Select(s => s.UserId).Distinct().Count();
                return uniqueUserIds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active users count");
                throw;
            }
        }

        public async Task<int> GetActiveSessionsCountAsync()
        {
            try
            {
                var sessions = await _unitOfWork.Sessions.GetActiveSessionsAsync();
                return sessions.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active sessions count");
                throw;
            }
        }

        public async Task<int> GetComputersInUseCountAsync()
        {
            try
            {
                var computers = await _unitOfWork.Computers.GetByStatusAsync(ComputerStatus.InUse);
                return computers.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving computers in use count");
                throw;
            }
        }

        public async Task<int> GetAvailableComputersCountAsync()
        {
            try
            {
                var computers = await _unitOfWork.Computers.GetAvailableComputersAsync();
                return computers.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available computers count");
                throw;
            }
        }
    }
}