using InternetCafe.Application.DTOs.Statistics;
using InternetCafe.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InternetCafe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "1,2")] // Staff and Admin only
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(
            IStatisticsService statisticsService,
            ILogger<StatisticsController> logger)
        {
            _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(StatisticsSummaryDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<StatisticsSummaryDTO>> GetStatisticsSummary()
        {
            try
            {
                var summary = await _statisticsService.GetStatisticsSummaryAsync();
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving statistics summary");
                return StatusCode(500, new { Message = "An error occurred while retrieving statistics summary" });
            }
        }

        [HttpGet("revenue")]
        [ProducesResponseType(typeof(RevenueSummaryDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<RevenueSummaryDTO>> GetRevenueSummary(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Default to current month if dates not specified
                var start = startDate ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var end = endDate ?? DateTime.UtcNow;

                if (start > end)
                {
                    return BadRequest(new { Message = "Start date must be before end date" });
                }

                var revenue = await _statisticsService.GetRevenueSummaryAsync(start, end);
                return Ok(revenue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving revenue summary");
                return StatusCode(500, new { Message = "An error occurred while retrieving revenue summary" });
            }
        }

        [HttpGet("usage")]
        [ProducesResponseType(typeof(UsageStatisticsDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<UsageStatisticsDTO>> GetUsageStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Default to current month if dates not specified
                var start = startDate ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var end = endDate ?? DateTime.UtcNow;

                if (start > end)
                {
                    return BadRequest(new { Message = "Start date must be before end date" });
                }

                var usage = await _statisticsService.GetUsageStatisticsAsync(start, end);
                return Ok(usage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving usage statistics");
                return StatusCode(500, new { Message = "An error occurred while retrieving usage statistics" });
            }
        }

        [HttpGet("top-users")]
        [ProducesResponseType(typeof(IEnumerable<TopUserDTO>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<IEnumerable<TopUserDTO>>> GetTopUsers(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int count = 10)
        {
            try
            {
                // Default to current month if dates not specified
                var start = startDate ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var end = endDate ?? DateTime.UtcNow;

                if (start > end)
                {
                    return BadRequest(new { Message = "Start date must be before end date" });
                }

                if (count <= 0)
                {
                    return BadRequest(new { Message = "Count must be greater than zero" });
                }

                var topUsers = await _statisticsService.GetTopUsersAsync(start, end, count);
                return Ok(topUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top users");
                return StatusCode(500, new { Message = "An error occurred while retrieving top users" });
            }
        }

        [HttpGet("daily-revenue")]
        [ProducesResponseType(typeof(IEnumerable<DailyRevenueDTO>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<IEnumerable<DailyRevenueDTO>>> GetDailyRevenue(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Default to current month if dates not specified
                var start = startDate ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var end = endDate ?? DateTime.UtcNow;

                if (start > end)
                {
                    return BadRequest(new { Message = "Start date must be before end date" });
                }

                var dailyRevenue = await _statisticsService.GetDailyRevenueAsync(start, end);
                return Ok(dailyRevenue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving daily revenue");
                return StatusCode(500, new { Message = "An error occurred while retrieving daily revenue" });
            }
        }

        [HttpGet("total-revenue")]
        [ProducesResponseType(typeof(decimal), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<decimal>> GetTotalRevenue(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Default to current month if dates not specified
                var start = startDate ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var end = endDate ?? DateTime.UtcNow;

                if (start > end)
                {
                    return BadRequest(new { Message = "Start date must be before end date" });
                }

                var totalRevenue = await _statisticsService.GetTotalRevenueAsync(start, end);
                return Ok(totalRevenue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total revenue");
                return StatusCode(500, new { Message = "An error occurred while retrieving total revenue" });
            }
        }

        [HttpGet("active-users-count")]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<int>> GetActiveUsersCount()
        {
            try
            {
                var count = await _statisticsService.GetActiveUsersCountAsync();
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active users count");
                return StatusCode(500, new { Message = "An error occurred while retrieving active users count" });
            }
        }

        [HttpGet("active-sessions-count")]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<int>> GetActiveSessionsCount()
        {
            try
            {
                var count = await _statisticsService.GetActiveSessionsCountAsync();
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active sessions count");
                return StatusCode(500, new { Message = "An error occurred while retrieving active sessions count" });
            }
        }

        [HttpGet("computers-in-use-count")]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<int>> GetComputersInUseCount()
        {
            try
            {
                var count = await _statisticsService.GetComputersInUseCountAsync();
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving computers in use count");
                return StatusCode(500, new { Message = "An error occurred while retrieving computers in use count" });
            }
        }

        [HttpGet("available-computers-count")]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<int>> GetAvailableComputersCount()
        {
            try
            {
                var count = await _statisticsService.GetAvailableComputersCountAsync();
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available computers count");
                return StatusCode(500, new { Message = "An error occurred while retrieving available computers count" });
            }
        }
    }
}