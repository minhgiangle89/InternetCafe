using InternetCafe.API.Common;
using InternetCafe.Application.DTOs.Authentication.Models;
using InternetCafe.Application.Interfaces.Services;
using InternetCafe.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace InternetCafe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthenticationService authService,
            ILogger<AuthController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthenticationResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<AuthenticationResponse>), 400)]
        [ProducesResponseType(typeof(ApiResponse<AuthenticationResponse>), 401)]
        [ProducesResponseType(typeof(ApiResponse<AuthenticationResponse>), 404)]
        public async Task<ActionResult<ApiResponse<AuthenticationResponse>>> Login([FromBody] AuthenticationRequest request)
        {
            try
            {
                var response = await _authService.AuthenticateAsync(request);
                return Ok(new ApiResponse<AuthenticationResponse>
                {
                    Success = true,
                    Message = "Đăng nhập thành công",
                    Data = response
                });
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Đăng nhập thất bại: Tài khoản {request.Username} không tồn tại");
                return NotFound(new ApiResponse<AuthenticationResponse>
                {
                    Success = false,
                    Message = "Tài khoản không tồn tại"
                });
            }
            catch (AuthenticationException ex)
            {
                _logger.LogWarning(ex, $"Đăng nhập thất bại: Sai thông tin đăng nhập cho tài khoản {request.Username}");
                return Unauthorized(new ApiResponse<AuthenticationResponse>
                {
                    Success = false,
                    Message = "Thông tin đăng nhập không chính xác"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi đăng nhập cho tài khoản {request.Username}");
                return BadRequest(new ApiResponse<AuthenticationResponse>
                {
                    Success = false,
                    Message = "Đăng nhập không thành công"
                });
            }
        }

        [HttpPost("refresh")]
        [ProducesResponseType(typeof(ApiResponse<AuthenticationResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<AuthenticationResponse>), 400)]
        [ProducesResponseType(typeof(ApiResponse<AuthenticationResponse>), 401)]
        public async Task<ActionResult<ApiResponse<AuthenticationResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var response = await _authService.RefreshTokenAsync(request);
                return Ok(new ApiResponse<AuthenticationResponse>
                {
                    Success = true,
                    Message = "Làm mới token thành công",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi làm mới token");
                return Unauthorized(new ApiResponse<AuthenticationResponse>
                {
                    Success = false,
                    Message = "Token không hợp lệ"
                });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 401)]
        public ActionResult<ApiResponse<bool>> Logout()
        {
            try
            {
                // JWT là stateless, nên logout chỉ đơn giản là thông báo thành công
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Đăng xuất thành công",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi đăng xuất");
                return Unauthorized(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Đăng xuất không thành công"
                });
            }
        }
    }
}