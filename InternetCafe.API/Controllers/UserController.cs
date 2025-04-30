using InternetCafe.Application.DTOs.User;
using InternetCafe.Application.Interfaces;
using InternetCafe.Application.Interfaces.Services;
using InternetCafe.Domain.Enums;
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
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService userService,
            IAccountService accountService,
            ICurrentUserService currentUserService,
            ILogger<UserController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [Authorize(Roles = "2")] // Admin only
        [ProducesResponseType(typeof(IEnumerable<UserDTO>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
                return StatusCode(500, new { Message = "An error occurred while retrieving users" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UserDTO>> GetUserById(int id)
        {
            try
            {
                // Ensure user can only access their own data unless they're admin
                var currentUserId = _currentUserService.UserId;
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (currentUserId != id && userRole != "2") // Not own profile and not admin
                {
                    return Forbid();
                }

                var user = await _userService.GetUserByIdAsync(id);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error retrieving user with ID {0}", id));
                return ex.Message.Contains("not found") ? NotFound(new { Message = ex.Message }) :
                    StatusCode(500, new { Message = "An error occurred while retrieving user" });
            }
        }

        [HttpGet("{id}/details")]
        [ProducesResponseType(typeof(UserDetailsDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UserDetailsDTO>> GetUserDetails(int id)
        {
            try
            {
                // Ensure user can only access their own data unless they're admin
                var currentUserId = _currentUserService.UserId;
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (currentUserId != id && userRole != "2") // Not own profile and not admin
                {
                    return Forbid();
                }

                var userDetails = await _userService.GetUserDetailsAsync(id);
                return Ok(userDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error retrieving user details for user with ID {0}", id));
                return ex.Message.Contains("not found") ? NotFound(new { Message = ex.Message }) :
                    StatusCode(500, new { Message = "An error occurred while retrieving user details" });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserDTO), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<UserDTO>> RegisterUser([FromBody] CreateUserDTO createUserDTO)
        {
            try
            {
                // If user is not logged in, they can only create customer accounts and vice versa
                // we won't use this feature temporarily, because this project for student
                //if (!User.Identity.IsAuthenticated)
                //{
                //    createUserDTO.Role = (int)UserRole.Customer;
                //}
                //else if (User.FindFirstValue(ClaimTypes.Role) != "2" && createUserDTO.Role == (int)UserRole.Admin)
                //{
                //    return Forbid();
                //}

                var user = await _userService.RegisterUserAsync(createUserDTO);

                await _accountService.CreateAccountAsync(user.Id);

                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error registering user {0}", createUserDTO.Username));
                return ex.Message.Contains("already exists") ?
                    Conflict(new { Message = ex.Message }) :
                    BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO updateUserDTO)
        {
            try
            {
                // Ensure user can only update their own data unless they're admin
                var currentUserId = _currentUserService.UserId;
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (currentUserId != id && userRole != "2") // Not own profile and not admin
                {
                    return Forbid();
                }

                await _userService.UpdateUserAsync(id, updateUserDTO);
                return Ok(new { Message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error updating user with ID {0}", id));
                return ex.Message.Contains("not found") ? NotFound(new { Message = ex.Message }) :
                    BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{id}/change-password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> ChangePassword(int id, [FromBody] ChangePasswordDTO changePasswordDTO)
        {
            try
            {
                // Ensure user can only change their own password
                var currentUserId = _currentUserService.UserId;
                if (currentUserId != id)
                {
                    return Forbid();
                }

                // Validate password match
                if (changePasswordDTO.NewPassword != changePasswordDTO.ConfirmPassword)
                {
                    return BadRequest(new { Message = "New password and confirmation do not match" });
                }

                await _userService.ChangePasswordAsync(id, changePasswordDTO);
                return Ok(new { Message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error changing password for user with ID {0}", id));

                if (ex.Message.Contains("not found"))
                    return NotFound(new { Message = ex.Message });
                else if (ex.Message.Contains("incorrect"))
                    return BadRequest(new { Message = "Current password is incorrect" });
                else
                    return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "2")] // Admin only
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> ChangeUserStatus(int id, [FromBody] int status)
        {
            try
            {
                if (!Enum.IsDefined(typeof(UserStatus), status))
                {
                    return BadRequest(new { Message = "Invalid status value" });
                }

                await _userService.ChangeUserStatusAsync(id, (UserStatus)status);
                return Ok(new { Message = "User status changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error changing status for user with ID {0}", id));
                return ex.Message.Contains("not found") ? NotFound(new { Message = ex.Message }) :
                    BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("current")]
        [ProducesResponseType(typeof(UserDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UserDTO>> GetCurrentUser()
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                var user = await _userService.GetUserByIdAsync(userId.Value);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user");
                return ex.Message.Contains("not found") ? NotFound(new { Message = ex.Message }) :
                    StatusCode(500, new { Message = "An error occurred while retrieving current user" });
            }
        }
    }
}