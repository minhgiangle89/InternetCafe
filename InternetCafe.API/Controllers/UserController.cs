using InternetCafe.API.Common;
using InternetCafe.Application.DTOs.User;
using InternetCafe.Application.Interfaces;
using InternetCafe.Application.Interfaces.Services;
using InternetCafe.Domain.Enums;
using InternetCafe.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
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
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDTO>>), 401)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDTO>>), 403)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDTO>>), 500)]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserDTO>>>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(ApiResponseFactory.Success(users, "Danh sách người dùng được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách người dùng");
                return StatusCode(500, ApiResponseFactory.Fail<IEnumerable<UserDTO>>("Lỗi server khi lấy danh sách người dùng"));
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<UserDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<UserDTO>), 403)]
        [ProducesResponseType(typeof(ApiResponse<UserDTO>), 404)]
        [ProducesResponseType(typeof(ApiResponse<UserDTO>), 500)]
        public async Task<ActionResult<ApiResponse<UserDTO>>> GetUserById(int id)
        {
            try
            {
                var currentUserId = _currentUserService.UserId;
                if (currentUserId != id && !User.IsInRole("2"))
                {
                    return Forbid();
                }

                var user = await _userService.GetUserByIdAsync(id);
                return Ok(ApiResponseFactory.Success(user, "Thông tin người dùng được tải thành công"));
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogWarning(ex, "Không tìm thấy người dùng có ID {UserId}", id);
                return NotFound(ApiResponseFactory.Fail<UserDTO>($"Không tìm thấy người dùng có ID {id}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin người dùng có ID {UserId}", id);
                return StatusCode(500, ApiResponseFactory.Fail<UserDTO>("Lỗi server khi lấy thông tin người dùng"));
            }
        }

        [HttpGet("{id}/details")]
        [ProducesResponseType(typeof(ApiResponse<UserDetailsDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<UserDetailsDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<UserDetailsDTO>), 403)]
        [ProducesResponseType(typeof(ApiResponse<UserDetailsDTO>), 404)]
        [ProducesResponseType(typeof(ApiResponse<UserDetailsDTO>), 500)]
        public async Task<ActionResult<ApiResponse<UserDetailsDTO>>> GetUserDetails(int id)
        {
            try
            {
                var currentUserId = _currentUserService.UserId;
                if (currentUserId != id && !User.IsInRole("2"))
                {
                    return Forbid();
                }

                var userDetails = await _userService.GetUserDetailsAsync(id);
                return Ok(ApiResponseFactory.Success(userDetails, "Chi tiết người dùng được tải thành công"));
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogWarning(ex, "Không tìm thấy người dùng có ID {UserId}", id);
                return NotFound(ApiResponseFactory.Fail<UserDetailsDTO>($"Không tìm thấy người dùng có ID {id}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết người dùng có ID {UserId}", id);
                return StatusCode(500, ApiResponseFactory.Fail<UserDetailsDTO>("Lỗi server khi lấy chi tiết người dùng"));
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<UserDTO>), 201)]
        [ProducesResponseType(typeof(ApiResponse<UserDTO>), 400)]
        [ProducesResponseType(typeof(ApiResponse<UserDTO>), 409)]
        [ProducesResponseType(typeof(ApiResponse<UserDTO>), 500)]
        public async Task<ActionResult<ApiResponse<UserDTO>>> RegisterUser([FromBody] CreateUserDTO createUserDTO)
        {
            try
            {
                //if (!User.Identity.IsAuthenticated)
                //{
                //    createUserDTO.Role = (int)UserRole.Customer;
                //}
                //else if (!User.IsInRole("2") && createUserDTO.Role == (int)UserRole.Admin)
                //{
                //    return Forbid();
                //}

                var user = await _userService.RegisterUserAsync(createUserDTO);
                await _accountService.CreateAccountAsync(user.Id);

                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, 
                    ApiResponseFactory.Success(user, "Đăng ký người dùng thành công"));
            }
            catch (DuplicateUserException ex)
            {
                _logger.LogWarning(ex, "Đăng ký người dùng thất bại: Trùng lặp thông tin");
                return Conflict(ApiResponseFactory.Fail<UserDTO>(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng ký người dùng {Username}", createUserDTO.Username);
                return BadRequest(ApiResponseFactory.Fail<UserDTO>("Đăng ký người dùng không thành công"));
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponseBase), 200)]
        [ProducesResponseType(typeof(ApiResponseBase), 401)]
        [ProducesResponseType(typeof(ApiResponseBase), 403)]
        [ProducesResponseType(typeof(ApiResponseBase), 404)]
        [ProducesResponseType(typeof(ApiResponseBase), 400)]
        [ProducesResponseType(typeof(ApiResponseBase), 500)]
        public async Task<ActionResult<ApiResponseBase>> UpdateUser(int id, [FromBody] UpdateUserDTO updateUserDTO)
        {
            try
            {
                var currentUserId = _currentUserService.UserId;
                if (currentUserId != id && !User.IsInRole("2"))
                {
                    return Forbid();
                }

                await _userService.UpdateUserAsync(id, updateUserDTO);
                return Ok(ApiResponseFactory.Success("Cập nhật thông tin người dùng thành công"));
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogWarning(ex, "Không tìm thấy người dùng có ID {UserId}", id);
                return NotFound(ApiResponseFactory.Fail($"Không tìm thấy người dùng có ID {id}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật thông tin người dùng có ID {UserId}", id);
                return BadRequest(ApiResponseFactory.Fail("Cập nhật thông tin người dùng không thành công"));
            }
        }

        [HttpPut("{id}/change-password")]
        [ProducesResponseType(typeof(ApiResponseBase), 200)]
        [ProducesResponseType(typeof(ApiResponseBase), 401)]
        [ProducesResponseType(typeof(ApiResponseBase), 403)]
        [ProducesResponseType(typeof(ApiResponseBase), 404)]
        [ProducesResponseType(typeof(ApiResponseBase), 400)]
        public async Task<ActionResult<ApiResponseBase>> ChangePassword(int id, [FromBody] ChangePasswordDTO changePasswordDTO)
        {
            try
            {
                var currentUserId = _currentUserService.UserId;
                if (currentUserId != id)
                {
                    return Forbid();
                }

                if (changePasswordDTO.NewPassword != changePasswordDTO.ConfirmPassword)
                {
                    return BadRequest(ApiResponseFactory.Fail("Mật khẩu mới và xác nhận mật khẩu không khớp"));
                }

                await _userService.ChangePasswordAsync(id, changePasswordDTO);
                return Ok(ApiResponseFactory.Success("Thay đổi mật khẩu thành công"));
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogWarning(ex, "Không tìm thấy người dùng có ID {UserId}", id);
                return NotFound(ApiResponseFactory.Fail($"Không tìm thấy người dùng có ID {id}"));
            }
            catch (AuthenticationException ex)
            {
                _logger.LogWarning(ex, "Mật khẩu hiện tại không chính xác cho người dùng ID {UserId}", id);
                return BadRequest(ApiResponseFactory.Fail("Mật khẩu hiện tại không chính xác"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thay đổi mật khẩu cho người dùng có ID {UserId}", id);
                return BadRequest(ApiResponseFactory.Fail("Thay đổi mật khẩu không thành công"));
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "2")] // Admin only
        [ProducesResponseType(typeof(ApiResponseBase), 200)]
        [ProducesResponseType(typeof(ApiResponseBase), 401)]
        [ProducesResponseType(typeof(ApiResponseBase), 403)]
        [ProducesResponseType(typeof(ApiResponseBase), 404)]
        [ProducesResponseType(typeof(ApiResponseBase), 400)]
        public async Task<ActionResult<ApiResponseBase>> ChangeUserStatus(int id, [FromBody] int status)
        {
            try
            {
                if (!Enum.IsDefined(typeof(UserStatus), status))
                {
                    return BadRequest(ApiResponseFactory.Fail("Giá trị trạng thái không hợp lệ"));
                }

                await _userService.ChangeUserStatusAsync(id, (UserStatus)status);
                return Ok(ApiResponseFactory.Success("Thay đổi trạng thái người dùng thành công"));
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogWarning(ex, "Không tìm thấy người dùng có ID {UserId}", id);
                return NotFound(ApiResponseFactory.Fail($"Không tìm thấy người dùng có ID {id}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thay đổi trạng thái người dùng có ID {UserId}", id);
                return BadRequest(ApiResponseFactory.Fail("Thay đổi trạng thái người dùng không thành công"));
            }
        }

        [HttpGet("current")]
        [ProducesResponseType(typeof(ApiResponse<UserDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<UserDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<UserDTO>), 404)]
        public async Task<ActionResult<ApiResponse<UserDTO>>> GetCurrentUser()
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (!userId.HasValue)
                {
                    return Unauthorized(ApiResponseFactory.Fail<UserDTO>("Người dùng chưa đăng nhập"));
                }

                var user = await _userService.GetUserByIdAsync(userId.Value);
                return Ok(ApiResponseFactory.Success(user, "Thông tin người dùng hiện tại được tải thành công"));
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogWarning(ex, "Không tìm thấy thông tin người dùng hiện tại");
                return NotFound(ApiResponseFactory.Fail<UserDTO>("Không tìm thấy thông tin người dùng hiện tại"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin người dùng hiện tại");
                return StatusCode(500, ApiResponseFactory.Fail<UserDTO>("Lỗi server khi lấy thông tin người dùng hiện tại"));
            }
        }
    }
}