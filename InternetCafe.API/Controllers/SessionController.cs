using InternetCafe.API.Common;
using InternetCafe.Application.DTOs.Session;
using InternetCafe.Application.Interfaces;
using InternetCafe.Application.Interfaces.Services;
using InternetCafe.Domain.Exceptions;
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
    [Authorize]
    public class SessionController : ControllerBase
    {
        private readonly ISessionService _sessionService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<SessionController> _logger;

        public SessionController(
            ISessionService sessionService,
            ICurrentUserService currentUserService,
            ILogger<SessionController> logger)
        {
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [Authorize(Roles = "1,2")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionDTO>>), 401)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionDTO>>), 403)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionDTO>>), 500)]
        public async Task<ActionResult<ApiResponse<IEnumerable<SessionDTO>>>> GetActiveSessions()
        {
            try
            {
                var sessions = await _sessionService.GetActiveSessionsAsync();
                return Ok(ApiResponseFactory.Success(sessions, "Danh sách phiên hoạt động được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phiên hoạt động");
                return StatusCode(500, ApiResponseFactory.Fail<IEnumerable<SessionDTO>>("Lỗi server khi lấy danh sách phiên hoạt động"));
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 403)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 404)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 500)]
        public async Task<ActionResult<ApiResponse<SessionDTO>>> GetSessionById(int id)
        {
            try
            {
                var session = await _sessionService.GetSessionDetailsAsync(id);

                var currentUserId = _currentUserService.UserId;
                if (session.UserId != currentUserId && !User.IsInRole("1") && !User.IsInRole("2"))
                {
                    return Forbid();
                }

                return Ok(ApiResponseFactory.Success(session, "Thông tin phiên được tải thành công"));
            }
            catch (SessionNotFoundException ex)
            {
                _logger.LogWarning(ex, "Không tìm thấy phiên có ID {SessionId}", id);
                return NotFound(ApiResponseFactory.Fail<SessionDTO>($"Không tìm thấy phiên có ID {id}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin phiên có ID {SessionId}", id);
                return StatusCode(500, ApiResponseFactory.Fail<SessionDTO>("Lỗi server khi lấy thông tin phiên"));
            }
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionDTO>>), 401)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionDTO>>), 403)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionDTO>>), 404)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionDTO>>), 500)]
        public async Task<ActionResult<ApiResponse<IEnumerable<SessionDTO>>>> GetSessionsByUserId(int userId)
        {
            try
            {
                var currentUserId = _currentUserService.UserId;
                if (userId != currentUserId && !User.IsInRole("1") && !User.IsInRole("2"))
                {
                    return Forbid();
                }

                var sessions = await _sessionService.GetSessionsByUserIdAsync(userId);
                return Ok(ApiResponseFactory.Success(sessions, "Danh sách phiên của người dùng được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phiên cho người dùng có ID {UserId}", userId);
                return StatusCode(500, ApiResponseFactory.Fail<IEnumerable<SessionDTO>>("Lỗi server khi lấy danh sách phiên của người dùng"));
            }
        }

        [HttpGet("computer/{computerId}")]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 404)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 500)]
        public async Task<ActionResult<ApiResponse<SessionDTO>>> GetActiveSessionByComputerId(int computerId)
        {
            try
            {
                var session = await _sessionService.GetActiveSessionByComputerIdAsync(computerId);
                if (session == null)
                {
                    return NotFound(ApiResponseFactory.Fail<SessionDTO>($"Không tìm thấy phiên hoạt động cho máy tính có ID {computerId}"));
                }

                var currentUserId = _currentUserService.UserId;
                if (session.UserId != currentUserId && !User.IsInRole("1") && !User.IsInRole("2"))
                {
                    return Forbid();
                }

                return Ok(ApiResponseFactory.Success(session, "Phiên hoạt động của máy tính được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy phiên hoạt động cho máy tính có ID {ComputerId}", computerId);
                return StatusCode(500, ApiResponseFactory.Fail<SessionDTO>("Lỗi server khi lấy phiên hoạt động của máy tính"));
            }
        }

        [HttpPost("start")]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 201)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 400)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 403)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 404)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 500)]
        public async Task<ActionResult<ApiResponse<SessionDTO>>> StartSession([FromBody] StartSessionDTO startSessionDTO)
        {
            try
            {
                var currentUserId = _currentUserService.UserId;
                if (startSessionDTO.UserId != currentUserId && !User.IsInRole("1") && !User.IsInRole("2"))
                {
                    return Forbid();
                }

                var session = await _sessionService.StartSessionAsync(startSessionDTO);
                return CreatedAtAction(nameof(GetSessionById), new { id = session.Id },
                    ApiResponseFactory.Success(session, "Bắt đầu phiên thành công"));
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogWarning(ex, "Không tìm thấy người dùng có ID {UserId}", startSessionDTO.UserId);
                return NotFound(ApiResponseFactory.Fail<SessionDTO>(ex.Message));
            }
            catch (ComputerNotAvailableException ex)
            {
                _logger.LogWarning(ex, "Máy tính có ID {ComputerId} không khả dụng", startSessionDTO.ComputerId);
                return BadRequest(ApiResponseFactory.Fail<SessionDTO>(ex.Message));
            }
            catch (InsufficientBalanceException ex)
            {
                _logger.LogWarning(ex, "Số dư không đủ để bắt đầu phiên cho người dùng có ID {UserId}", startSessionDTO.UserId);
                return BadRequest(ApiResponseFactory.Fail<SessionDTO>(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi bắt đầu phiên cho người dùng {UserId} trên máy tính {ComputerId}",
                    startSessionDTO.UserId, startSessionDTO.ComputerId);

                if (ex.Message.Contains("already has an active session"))
                    return BadRequest(ApiResponseFactory.Fail<SessionDTO>(ex.Message));

                return StatusCode(500, ApiResponseFactory.Fail<SessionDTO>("Lỗi server khi bắt đầu phiên"));
            }
        }

        [HttpPost("end")]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 403)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 404)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 400)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 500)]
        public async Task<ActionResult<ApiResponse<SessionDTO>>> EndSession([FromBody] EndSessionDTO endSessionDTO)
        {
            try
            {
                var session = await _sessionService.GetSessionDetailsAsync(endSessionDTO.SessionId);

                var currentUserId = _currentUserService.UserId;
                if (session.UserId != currentUserId && !User.IsInRole("1") && !User.IsInRole("2"))
                {
                    return Forbid();
                }

                var endedSession = await _sessionService.EndSessionAsync(endSessionDTO);
                return Ok(ApiResponseFactory.Success(endedSession, "Kết thúc phiên thành công"));
            }
            catch (SessionNotFoundException ex)
            {
                _logger.LogWarning(ex, "Không tìm thấy phiên có ID {SessionId}", endSessionDTO.SessionId);
                return NotFound(ApiResponseFactory.Fail<SessionDTO>(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kết thúc phiên có ID {SessionId}", endSessionDTO.SessionId);

                if (ex.Message.Contains("not active"))
                    return BadRequest(ApiResponseFactory.Fail<SessionDTO>(ex.Message));

                return StatusCode(500, ApiResponseFactory.Fail<SessionDTO>("Lỗi server khi kết thúc phiên"));
            }
        }

        [HttpPost("{id}/terminate")]
        [Authorize(Roles = "1,2")]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 403)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 404)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 400)]
        [ProducesResponseType(typeof(ApiResponse<SessionDTO>), 500)]
        public async Task<ActionResult<ApiResponse<SessionDTO>>> TerminateSession(int id, [FromBody] string reason)
        {
            try
            {
                var session = await _sessionService.TerminateSessionAsync(id, reason);
                return Ok(ApiResponseFactory.Success(session, "Dừng phiên thành công"));
            }
            catch (SessionNotFoundException ex)
            {
                _logger.LogWarning(ex, "Không tìm thấy phiên có ID {SessionId}", id);
                return NotFound(ApiResponseFactory.Fail<SessionDTO>(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi dừng phiên có ID {SessionId}", id);

                if (ex.Message.Contains("not active"))
                    return BadRequest(ApiResponseFactory.Fail<SessionDTO>(ex.Message));

                return StatusCode(500, ApiResponseFactory.Fail<SessionDTO>("Lỗi server khi dừng phiên"));
            }
        }

        [HttpGet("{id}/cost")]
        [ProducesResponseType(typeof(ApiResponse<decimal>), 200)]
        [ProducesResponseType(typeof(ApiResponse<decimal>), 401)]
        [ProducesResponseType(typeof(ApiResponse<decimal>), 403)]
        [ProducesResponseType(typeof(ApiResponse<decimal>), 404)]
        [ProducesResponseType(typeof(ApiResponse<decimal>), 500)]
        public async Task<ActionResult<ApiResponse<decimal>>> CalculateSessionCost(int id)
        {
            try
            {
                var session = await _sessionService.GetSessionDetailsAsync(id);
                var currentUserId = _currentUserService.UserId;
                if (session.UserId != currentUserId && !User.IsInRole("1") && !User.IsInRole("2"))
                {
                    return Forbid();
                }

                var cost = await _sessionService.CalculateSessionCostAsync(id);
                return Ok(ApiResponseFactory.Success(cost, "Tính phí phiên thành công"));
            }
            catch (SessionNotFoundException ex)
            {
                _logger.LogWarning(ex, "Không tìm thấy phiên có ID {SessionId}", id);
                return NotFound(ApiResponseFactory.Fail<decimal>(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tính phí phiên có ID {SessionId}", id);
                return StatusCode(500, ApiResponseFactory.Fail<decimal>("Lỗi server khi tính phí phiên"));
            }
        }

        [HttpGet("user/{userId}/computer/{computerId}/remaining-time")]
        [ProducesResponseType(typeof(ApiResponse<TimeSpan>), 200)]
        [ProducesResponseType(typeof(ApiResponse<TimeSpan>), 401)]
        [ProducesResponseType(typeof(ApiResponse<TimeSpan>), 403)]
        [ProducesResponseType(typeof(ApiResponse<TimeSpan>), 404)]
        [ProducesResponseType(typeof(ApiResponse<TimeSpan>), 500)]
        public async Task<ActionResult<ApiResponse<TimeSpan>>> GetRemainingTime(int userId, int computerId)
        {
            try
            {
                var currentUserId = _currentUserService.UserId;
                if (userId != currentUserId && !User.IsInRole("1") && !User.IsInRole("2"))
                {
                    return Forbid();
                }

                var remainingTime = await _sessionService.GetRemainingTimeAsync(userId, computerId);
                return Ok(ApiResponseFactory.Success(remainingTime, "Lấy thời gian sử dụng còn lại thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thời gian sử dụng còn lại cho người dùng {UserId} trên máy tính {ComputerId}",
                    userId, computerId);

                if (ex.Message.Contains("No active session"))
                    return NotFound(ApiResponseFactory.Fail<TimeSpan>(ex.Message));

                return StatusCode(500, ApiResponseFactory.Fail<TimeSpan>("Lỗi server khi lấy thời gian sử dụng còn lại"));
            }
        }

        [HttpGet("user/{userId}/has-active-session")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 401)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 403)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 500)]
        public async Task<ActionResult<ApiResponse<bool>>> HasActiveSession(int userId)
        {
            try
            {
                var currentUserId = _currentUserService.UserId;
                if (userId != currentUserId && !User.IsInRole("1") && !User.IsInRole("2"))
                {
                    return Forbid();
                }

                var hasActiveSession = await _sessionService.HasActiveSessionAsync(userId);
                return Ok(ApiResponseFactory.Success(hasActiveSession, "Kiểm tra phiên hoạt động thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra phiên hoạt động cho người dùng có ID {UserId}", userId);
                return StatusCode(500, ApiResponseFactory.Fail<bool>("Lỗi server khi kiểm tra phiên hoạt động"));
            }
        }

        [HttpGet("date-range")]
        [Authorize(Roles = "1,2")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionDTO>>), 401)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionDTO>>), 403)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionDTO>>), 400)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionDTO>>), 500)]
        public async Task<ActionResult<ApiResponse<IEnumerable<SessionDTO>>>> GetSessionsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest(ApiResponseFactory.Fail<IEnumerable<SessionDTO>>("Ngày bắt đầu phải trước ngày kết thúc"));
                }

                var sessions = await _sessionService.GetSessionsByDateRangeAsync(startDate, endDate);
                return Ok(ApiResponseFactory.Success(sessions, "Danh sách phiên theo khoảng thời gian được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phiên theo khoảng thời gian từ {StartDate} đến {EndDate}",
                    startDate, endDate);
                return StatusCode(500, ApiResponseFactory.Fail<IEnumerable<SessionDTO>>("Lỗi server khi lấy danh sách phiên theo khoảng thời gian"));
            }
        }
    }
}