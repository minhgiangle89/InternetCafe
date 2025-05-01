using InternetCafe.API.Common;
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
    [Authorize(Roles = "1,2")]
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
        [ProducesResponseType(typeof(ApiResponse<StatisticsSummaryDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<StatisticsSummaryDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<StatisticsSummaryDTO>), 403)]
        [ProducesResponseType(typeof(ApiResponse<StatisticsSummaryDTO>), 500)]
        public async Task<ActionResult<ApiResponse<StatisticsSummaryDTO>>> GetStatisticsSummary()
        {
            try
            {
                var summary = await _statisticsService.GetStatisticsSummaryAsync();
                return Ok(ApiResponseFactory.Success(summary, "Thông tin thống kê tổng quan được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin thống kê tổng quan");
                return StatusCode(500, ApiResponseFactory.Fail<StatisticsSummaryDTO>("Lỗi server khi lấy thông tin thống kê tổng quan"));
            }
        }

        [HttpGet("revenue")]
        [ProducesResponseType(typeof(ApiResponse<RevenueSummaryDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<RevenueSummaryDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<RevenueSummaryDTO>), 403)]
        [ProducesResponseType(typeof(ApiResponse<RevenueSummaryDTO>), 400)]
        [ProducesResponseType(typeof(ApiResponse<RevenueSummaryDTO>), 500)]
        public async Task<ActionResult<ApiResponse<RevenueSummaryDTO>>> GetRevenueSummary(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var end = endDate ?? DateTime.UtcNow;

                if (start > end)
                {
                    return BadRequest(ApiResponseFactory.Fail<RevenueSummaryDTO>("Ngày bắt đầu phải trước ngày kết thúc"));
                }

                var revenue = await _statisticsService.GetRevenueSummaryAsync(start, end);
                return Ok(ApiResponseFactory.Success(revenue, "Thống kê doanh thu được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê doanh thu");
                return StatusCode(500, ApiResponseFactory.Fail<RevenueSummaryDTO>("Lỗi server khi lấy thống kê doanh thu"));
            }
        }

        [HttpGet("usage")]
        [ProducesResponseType(typeof(ApiResponse<UsageStatisticsDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<UsageStatisticsDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<UsageStatisticsDTO>), 403)]
        [ProducesResponseType(typeof(ApiResponse<UsageStatisticsDTO>), 400)]
        [ProducesResponseType(typeof(ApiResponse<UsageStatisticsDTO>), 500)]
        public async Task<ActionResult<ApiResponse<UsageStatisticsDTO>>> GetUsageStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var end = endDate ?? DateTime.UtcNow;

                if (start > end)
                {
                    return BadRequest(ApiResponseFactory.Fail<UsageStatisticsDTO>("Ngày bắt đầu phải trước ngày kết thúc"));
                }

                var usage = await _statisticsService.GetUsageStatisticsAsync(start, end);
                return Ok(ApiResponseFactory.Success(usage, "Thống kê sử dụng được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê sử dụng");
                return StatusCode(500, ApiResponseFactory.Fail<UsageStatisticsDTO>("Lỗi server khi lấy thống kê sử dụng"));
            }
        }

        [HttpGet("top-users")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TopUserDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TopUserDTO>>), 401)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TopUserDTO>>), 403)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TopUserDTO>>), 400)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TopUserDTO>>), 500)]
        public async Task<ActionResult<ApiResponse<IEnumerable<TopUserDTO>>>> GetTopUsers(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int count = 10)
        {
            try
            {
                var start = startDate ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var end = endDate ?? DateTime.UtcNow;

                if (start > end)
                {
                    return BadRequest(ApiResponseFactory.Fail<IEnumerable<TopUserDTO>>("Ngày bắt đầu phải trước ngày kết thúc"));
                }

                if (count <= 0)
                {
                    return BadRequest(ApiResponseFactory.Fail<IEnumerable<TopUserDTO>>("Số lượng phải lớn hơn 0"));
                }

                var topUsers = await _statisticsService.GetTopUsersAsync(start, end, count);
                return Ok(ApiResponseFactory.Success(topUsers, "Danh sách người dùng hàng đầu được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách người dùng hàng đầu");
                return StatusCode(500, ApiResponseFactory.Fail<IEnumerable<TopUserDTO>>("Lỗi server khi lấy danh sách người dùng hàng đầu"));
            }
        }

        [HttpGet("daily-revenue")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DailyRevenueDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DailyRevenueDTO>>), 401)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DailyRevenueDTO>>), 403)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DailyRevenueDTO>>), 400)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DailyRevenueDTO>>), 500)]
        public async Task<ActionResult<ApiResponse<IEnumerable<DailyRevenueDTO>>>> GetDailyRevenue(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var end = endDate ?? DateTime.UtcNow;

                if (start > end)
                {
                    return BadRequest(ApiResponseFactory.Fail<IEnumerable<DailyRevenueDTO>>("Ngày bắt đầu phải trước ngày kết thúc"));
                }

                var dailyRevenue = await _statisticsService.GetDailyRevenueAsync(start, end);
                return Ok(ApiResponseFactory.Success(dailyRevenue, "Doanh thu hàng ngày được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy doanh thu hàng ngày");
                return StatusCode(500, ApiResponseFactory.Fail<IEnumerable<DailyRevenueDTO>>("Lỗi server khi lấy doanh thu hàng ngày"));
            }
        }

        [HttpGet("total-revenue")]
        [ProducesResponseType(typeof(ApiResponse<decimal>), 200)]
        [ProducesResponseType(typeof(ApiResponse<decimal>), 401)]
        [ProducesResponseType(typeof(ApiResponse<decimal>), 403)]
        [ProducesResponseType(typeof(ApiResponse<decimal>), 400)]
        [ProducesResponseType(typeof(ApiResponse<decimal>), 500)]
        public async Task<ActionResult<ApiResponse<decimal>>> GetTotalRevenue(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var end = endDate ?? DateTime.UtcNow;

                if (start > end)
                {
                    return BadRequest(ApiResponseFactory.Fail<decimal>("Ngày bắt đầu phải trước ngày kết thúc"));
                }

                var totalRevenue = await _statisticsService.GetTotalRevenueAsync(start, end);
                return Ok(ApiResponseFactory.Success(totalRevenue, "Tổng doanh thu được tính thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tổng doanh thu");
                return StatusCode(500, ApiResponseFactory.Fail<decimal>("Lỗi server khi lấy tổng doanh thu"));
            }
        }

        [HttpGet("active-users-count")]
        [ProducesResponseType(typeof(ApiResponse<int>), 200)]
        [ProducesResponseType(typeof(ApiResponse<int>), 401)]
        [ProducesResponseType(typeof(ApiResponse<int>), 403)]
        [ProducesResponseType(typeof(ApiResponse<int>), 500)]
        public async Task<ActionResult<ApiResponse<int>>> GetActiveUsersCount()
        {
            try
            {
                var count = await _statisticsService.GetActiveUsersCountAsync();
                return Ok(ApiResponseFactory.Success(count, "Số lượng người dùng đang hoạt động được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy số lượng người dùng đang hoạt động");
                return StatusCode(500, ApiResponseFactory.Fail<int>("Lỗi server khi lấy số lượng người dùng đang hoạt động"));
            }
        }

        [HttpGet("active-sessions-count")]
        [ProducesResponseType(typeof(ApiResponse<int>), 200)]
        [ProducesResponseType(typeof(ApiResponse<int>), 401)]
        [ProducesResponseType(typeof(ApiResponse<int>), 403)]
        [ProducesResponseType(typeof(ApiResponse<int>), 500)]
        public async Task<ActionResult<ApiResponse<int>>> GetActiveSessionsCount()
        {
            try
            {
                var count = await _statisticsService.GetActiveSessionsCountAsync();
                return Ok(ApiResponseFactory.Success(count, "Số lượng phiên đang hoạt động được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy số lượng phiên đang hoạt động");
                return StatusCode(500, ApiResponseFactory.Fail<int>("Lỗi server khi lấy số lượng phiên đang hoạt động"));
            }
        }

        [HttpGet("computers-in-use-count")]
        [ProducesResponseType(typeof(ApiResponse<int>), 200)]
        [ProducesResponseType(typeof(ApiResponse<int>), 401)]
        [ProducesResponseType(typeof(ApiResponse<int>), 403)]
        [ProducesResponseType(typeof(ApiResponse<int>), 500)]
        public async Task<ActionResult<ApiResponse<int>>> GetComputersInUseCount()
        {
            try
            {
                var count = await _statisticsService.GetComputersInUseCountAsync();
                return Ok(ApiResponseFactory.Success(count, "Số lượng máy tính đang sử dụng được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy số lượng máy tính đang sử dụng");
                return StatusCode(500, ApiResponseFactory.Fail<int>("Lỗi server khi lấy số lượng máy tính đang sử dụng"));
            }
        }

        [HttpGet("available-computers-count")]
        [ProducesResponseType(typeof(ApiResponse<int>), 200)]
        [ProducesResponseType(typeof(ApiResponse<int>), 401)]
        [ProducesResponseType(typeof(ApiResponse<int>), 403)]
        [ProducesResponseType(typeof(ApiResponse<int>), 500)]
        public async Task<ActionResult<ApiResponse<int>>> GetAvailableComputersCount()
        {
            try
            {
                var count = await _statisticsService.GetAvailableComputersCountAsync();
                return Ok(ApiResponseFactory.Success(count, "Số lượng máy tính khả dụng được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy số lượng máy tính khả dụng");
                return StatusCode(500, ApiResponseFactory.Fail<int>("Lỗi server khi lấy số lượng máy tính khả dụng"));
            }
        }
    }
}