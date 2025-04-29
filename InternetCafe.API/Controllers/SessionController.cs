using InternetCafe.Application.DTOs.Session;
using InternetCafe.Application.Interfaces;
using InternetCafe.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
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
        [Authorize(Roles = "1,2")] // Staff and Admin only
        [ProducesResponseType(typeof(IEnumerable<SessionDTO>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<IEnumerable<SessionDTO>>> GetActiveSessions()
        {
            try
            {
                var sessions = await _sessionService.GetActiveSessionsAsync();
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active sessions");
                return StatusCode(500, new { Message = "An error occurred while retrieving active sessions" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SessionDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<SessionDTO>> GetSessionById(int id)
        {
            try
            {
                var session = await _sessionService.GetSessionDetailsAsync(id);

                // Ensure user can only access their own sessions unless they're staff or admin
                var currentUserId = _currentUserService.UserId;
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (session.UserId != currentUserId && userRole != "1" && userRole != "2") // Not own session and not staff/admin
                {
                    return Forbid();
                }

                return Ok(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving session with ID {SessionId}", id);
                return ex.Message.Contains("not found") ? NotFound(new { Message = ex.Message }) :
                    StatusCode(500, new { Message = "An error occurred while retrieving session" });
            }
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<SessionDTO>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<SessionDTO>>> GetSessionsByUserId(int userId)
        {
            try
            {
                // Ensure user can only access their own sessions unless they're staff or admin
                var currentUserId = _currentUserService.UserId;
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (userId != currentUserId && userRole != "1" && userRole != "2") // Not own sessions and not staff/admin
                {
                    return Forbid();
                }

                var sessions = await _sessionService.GetSessionsByUserIdAsync(userId);
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sessions for user with ID {UserId}", userId);
                return StatusCode(500, new { Message = "An error occurred while retrieving sessions" });
            }
        }

        [HttpGet("computer/{computerId}")]
        [ProducesResponseType(typeof(SessionDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<SessionDTO>> GetActiveSessionByComputerId(int computerId)
        {
            try
            {
                var session = await _sessionService.GetActiveSessionByComputerIdAsync(computerId);
                if (session == null)
                {
                    return NotFound(new { Message = $"No active session found for computer with ID {computerId}" });
                }

                // Ensure user can only access their own session unless they're staff or admin
                var currentUserId = _currentUserService.UserId;
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (session.UserId != currentUserId && userRole != "1" && userRole != "2") // Not own session and not staff/admin
                {
                    return Forbid();
                }

                return Ok(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active session for computer with ID {ComputerId}", computerId);
                return StatusCode(500, new { Message = "An error occurred while retrieving session" });
            }
        }

        [HttpPost("start")]
        [ProducesResponseType(typeof(SessionDTO), 201)]
        [ProducesResponseType(401)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<SessionDTO>> StartSession([FromBody] StartSessionDTO startSessionDTO)
        {
            try
            {
                // Ensure user can only start session for themselves unless they're staff or admin
                var currentUserId = _currentUserService.UserId;
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (startSessionDTO.UserId != currentUserId && userRole != "1" && userRole != "2") // Not own session and not staff/admin
                {
                    return Forbid();
                }

                var session = await _sessionService.StartSessionAsync(startSessionDTO);
                return CreatedAtAction(nameof(GetSessionById), new { id = session.Id }, session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting session for user {UserId} on computer {ComputerId}",
                    startSessionDTO.UserId, startSessionDTO.ComputerId);

                if (ex.Message.Contains("not found"))
                    return NotFound(new { Message = ex.Message });
                else if (ex.Message.Contains("not available"))
                    return BadRequest(new { Message = ex.Message });
                else if (ex.Message.Contains("already has an active session"))
                    return BadRequest(new { Message = ex.Message });
                else if (ex.Message.Contains("insufficient"))
                    return BadRequest(new { Message = ex.Message });
                else
                    return StatusCode(500, new { Message = "An error occurred while starting session" });
            }
        }

        [HttpPost("end")]
        [ProducesResponseType(typeof(SessionDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<SessionDTO>> EndSession([FromBody] EndSessionDTO endSessionDTO)
        {
            try
            {
                // First get the session to check permissions
                var session = await _sessionService.GetSessionDetailsAsync(endSessionDTO.SessionId);

                // Ensure user can only end their own sessions unless they're staff or admin
                var currentUserId = _currentUserService.UserId;
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (session.UserId != currentUserId && userRole != "1" && userRole != "2") // Not own session and not staff/admin
                {
                    return Forbid();
                }

                var endedSession = await _sessionService.EndSessionAsync(endSessionDTO);
                return Ok(endedSession);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending session with ID {SessionId}", endSessionDTO.SessionId);

                if (ex.Message.Contains("not found"))
                    return NotFound(new { Message = ex.Message });
                else if (ex.Message.Contains("not active"))
                    return BadRequest(new { Message = ex.Message });
                else
                    return StatusCode(500, new { Message = "An error occurred while ending session" });
            }
        }

        [HttpPost("{id}/terminate")]
        [Authorize(Roles = "1,2")] // Staff and Admin only
        [ProducesResponseType(typeof(SessionDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<SessionDTO>> TerminateSession(int id, [FromBody] string reason)
        {
            try
            {
                var session = await _sessionService.TerminateSessionAsync(id, reason);
                return Ok(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error terminating session with ID {SessionId}", id);

                if (ex.Message.Contains("not found"))
                    return NotFound(new { Message = ex.Message });
                else if (ex.Message.Contains("not active"))
                    return BadRequest(new { Message = ex.Message });
                else
                    return StatusCode(500, new { Message = "An error occurred while terminating session" });
            }
        }

        [HttpGet("{id}/cost")]
        [ProducesResponseType(typeof(decimal), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<decimal>> CalculateSessionCost(int id)
        {
            try
            {
                // First get the session to check permissions
                var session = await _sessionService.GetSessionDetailsAsync(id);

                // Ensure user can only access their own sessions unless they're staff or admin
                var currentUserId = _currentUserService.UserId;
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (session.UserId != currentUserId && userRole != "1" && userRole != "2") // Not own session and not staff/admin
                {
                    return Forbid();
                }

                var cost = await _sessionService.CalculateSessionCostAsync(id);
                return Ok(cost);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating cost for session with ID {SessionId}", id);

                if (ex.Message.Contains("not found"))
                    return NotFound(new { Message = ex.Message });
                else
                    return StatusCode(500, new { Message = "An error occurred while calculating session cost" });
            }
        }

        [HttpGet("user/{userId}/computer/{computerId}/remaining-time")]
        [ProducesResponseType(typeof(TimeSpan), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<TimeSpan>> GetRemainingTime(int userId, int computerId)
        {
            try
            {
                // Ensure user can only access their own remaining time unless they're staff or admin
                var currentUserId = _currentUserService.UserId;
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (userId != currentUserId && userRole != "1" && userRole != "2") // Not own time and not staff/admin
                {
                    return Forbid();
                }

                var remainingTime = await _sessionService.GetRemainingTimeAsync(userId, computerId);
                return Ok(remainingTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving remaining time for user {UserId} on computer {ComputerId}",
                    userId, computerId);

                if (ex.Message.Contains("No active session"))
                    return NotFound(new { Message = ex.Message });
                else
                    return StatusCode(500, new { Message = "An error occurred while retrieving remaining time" });
            }
        }

        [HttpGet("user/{userId}/has-active-session")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<bool>> HasActiveSession(int userId)
        {
            try
            {
                // Ensure user can only check their own session status unless they're staff or admin
                var currentUserId = _currentUserService.UserId;
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (userId != currentUserId && userRole != "1" && userRole != "2") // Not own status and not staff/admin
                {
                    return Forbid();
                }

                var hasActiveSession = await _sessionService.HasActiveSessionAsync(userId);
                return Ok(hasActiveSession);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} has active session", userId);
                return StatusCode(500, new { Message = "An error occurred while checking for active session" });
            }
        }

        [HttpGet("date-range")]
        [Authorize(Roles = "1,2")] // Staff and Admin only
        [ProducesResponseType(typeof(IEnumerable<SessionDTO>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<IEnumerable<SessionDTO>>> GetSessionsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest(new { Message = "Start date must be before end date" });
                }

                var sessions = await _sessionService.GetSessionsByDateRangeAsync(startDate, endDate);
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sessions for date range {StartDate} to {EndDate}",
                    startDate, endDate);
                return StatusCode(500, new { Message = "An error occurred while retrieving sessions" });
            }
        }
    }
}