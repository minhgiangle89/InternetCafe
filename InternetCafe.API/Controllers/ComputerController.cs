using InternetCafe.API.Common;
using InternetCafe.Application.DTOs.Computer;
using InternetCafe.Application.Interfaces.Services;
using InternetCafe.Domain.Enums;
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
    public class ComputerController : ControllerBase
    {
        private readonly IComputerService _computerService;
        private readonly ILogger<ComputerController> _logger;

        public ComputerController(
            IComputerService computerService,
            ILogger<ComputerController> logger)
        {
            _computerService = computerService ?? throw new ArgumentNullException(nameof(computerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ComputerDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ComputerDTO>>), 401)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ComputerDTO>>), 500)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ComputerDTO>>>> GetAllComputers()
        {
            try
            {
                var computers = await _computerService.GetAllComputersAsync();
                return Ok(ApiResponseFactory.Success(computers, "Danh sách máy tính được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách máy tính");
                return StatusCode(500, ApiResponseFactory.Fail<IEnumerable<ComputerDTO>>("Lỗi server khi lấy danh sách máy tính"));
            }
        }

        [HttpGet("available")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ComputerDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ComputerDTO>>), 401)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ComputerDTO>>), 500)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ComputerDTO>>>> GetAvailableComputers()
        {
            try
            {
                var computers = await _computerService.GetAvailableComputersAsync();
                return Ok(ApiResponseFactory.Success(computers, "Danh sách máy tính khả dụng được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách máy tính khả dụng");
                return StatusCode(500, ApiResponseFactory.Fail<IEnumerable<ComputerDTO>>("Lỗi server khi lấy danh sách máy tính khả dụng"));
            }
        }

        [HttpGet("status/{status}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ComputerDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ComputerDTO>>), 401)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ComputerDTO>>), 400)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ComputerDTO>>), 500)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ComputerDTO>>>> GetComputersByStatus(int status)
        {
            try
            {
                if (!Enum.IsDefined(typeof(ComputerStatus), status))
                {
                    return BadRequest(ApiResponseFactory.Fail<IEnumerable<ComputerDTO>>("Giá trị trạng thái không hợp lệ"));
                }

                var computers = await _computerService.GetComputersByStatusAsync((ComputerStatus)status);
                return Ok(ApiResponseFactory.Success(computers, $"Danh sách máy tính có trạng thái {(ComputerStatus)status} được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách máy tính theo trạng thái {Status}", status);
                return StatusCode(500, ApiResponseFactory.Fail<IEnumerable<ComputerDTO>>("Lỗi server khi lấy danh sách máy tính theo trạng thái"));
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 404)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 500)]
        public async Task<ActionResult<ApiResponse<ComputerDTO>>> GetComputerById(int id)
        {
            try
            {
                var computer = await _computerService.GetComputerByIdAsync(id);
                return Ok(ApiResponseFactory.Success(computer, "Thông tin máy tính được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin máy tính có ID {ComputerId}", id);

                if (ex.Message.Contains("not found"))
                    return NotFound(ApiResponseFactory.Fail<ComputerDTO>($"Không tìm thấy máy tính có ID {id}"));

                return StatusCode(500, ApiResponseFactory.Fail<ComputerDTO>("Lỗi server khi lấy thông tin máy tính"));
            }
        }

        [HttpGet("{id}/details")]
        [ProducesResponseType(typeof(ApiResponse<ComputerDetailsDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDetailsDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDetailsDTO>), 404)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDetailsDTO>), 500)]
        public async Task<ActionResult<ApiResponse<ComputerDetailsDTO>>> GetComputerDetails(int id)
        {
            try
            {
                var computerDetails = await _computerService.GetComputerDetailsAsync(id);
                return Ok(ApiResponseFactory.Success(computerDetails, "Chi tiết máy tính được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết máy tính có ID {ComputerId}", id);

                if (ex.Message.Contains("not found"))
                    return NotFound(ApiResponseFactory.Fail<ComputerDetailsDTO>($"Không tìm thấy máy tính có ID {id}"));

                return StatusCode(500, ApiResponseFactory.Fail<ComputerDetailsDTO>("Lỗi server khi lấy chi tiết máy tính"));
            }
        }

        [HttpPost]
        [Authorize(Roles = "1,2")]
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 201)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 403)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 400)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 500)]
        public async Task<ActionResult<ApiResponse<ComputerDTO>>> RegisterComputer([FromBody] CreateComputerDTO createComputerDTO)
        {
            try
            {
                var computer = await _computerService.RegisterComputerAsync(createComputerDTO);
                return CreatedAtAction(nameof(GetComputerById), new { id = computer.Id },
                    ApiResponseFactory.Success(computer, "Đăng ký máy tính mới thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng ký máy tính mới với tên {ComputerName}", createComputerDTO.Name);

                if (ex.Message.Contains("already exists") || ex.Message.Contains("Invalid"))
                    return BadRequest(ApiResponseFactory.Fail<ComputerDTO>(ex.Message));

                return StatusCode(500, ApiResponseFactory.Fail<ComputerDTO>("Lỗi server khi đăng ký máy tính mới"));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "1,2")] // Chỉ nhân viên và admin có thể cập nhật máy tính
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 403)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 404)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 400)]
        [ProducesResponseType(typeof(ApiResponse<ComputerDTO>), 500)]
        public async Task<ActionResult<ApiResponse<ComputerDTO>>> UpdateComputer(int id, [FromBody] UpdateComputerDTO updateComputerDTO)
        {
            try
            {
                var computer = await _computerService.UpdateComputerAsync(id, updateComputerDTO);
                return Ok(ApiResponseFactory.Success(computer, "Cập nhật máy tính thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật máy tính có ID {ComputerId}", id);

                if (ex.Message.Contains("not found"))
                    return NotFound(ApiResponseFactory.Fail<ComputerDTO>($"Không tìm thấy máy tính có ID {id}"));
                else if (ex.Message.Contains("already exists") || ex.Message.Contains("Invalid"))
                    return BadRequest(ApiResponseFactory.Fail<ComputerDTO>(ex.Message));

                return StatusCode(500, ApiResponseFactory.Fail<ComputerDTO>("Lỗi server khi cập nhật máy tính"));
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "1,2")]
        [ProducesResponseType(typeof(ApiResponseBase), 200)]
        [ProducesResponseType(typeof(ApiResponseBase), 401)]
        [ProducesResponseType(typeof(ApiResponseBase), 403)]
        [ProducesResponseType(typeof(ApiResponseBase), 404)]
        [ProducesResponseType(typeof(ApiResponseBase), 400)]
        [ProducesResponseType(typeof(ApiResponseBase), 500)]
        public async Task<ActionResult<ApiResponseBase>> UpdateComputerStatus(int id, [FromBody] ComputerStatusUpdateDTO updateDTO)
        {
            try
            {
                if (!Enum.IsDefined(typeof(ComputerStatus), updateDTO.Status))
                {
                    return BadRequest(ApiResponseFactory.Fail("Giá trị trạng thái không hợp lệ"));
                }

                if (id != updateDTO.ComputerId)
                {
                    updateDTO.ComputerId = id;
                }

                await _computerService.SetComputerStatusAsync(updateDTO);
                return Ok(ApiResponseFactory.Success("Cập nhật trạng thái máy tính thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái máy tính có ID {ComputerId}", id);

                if (ex.Message.Contains("not found"))
                    return NotFound(ApiResponseFactory.Fail($"Không tìm thấy máy tính có ID {id}"));
                else if (ex.Message.Contains("active session"))
                    return BadRequest(ApiResponseFactory.Fail(ex.Message));

                return StatusCode(500, ApiResponseFactory.Fail("Lỗi server khi cập nhật trạng thái máy tính"));
            }
        }

        [HttpPost("{id}/maintenance")]
        [Authorize(Roles = "1,2")]
        [ProducesResponseType(typeof(ApiResponseBase), 200)]
        [ProducesResponseType(typeof(ApiResponseBase), 401)]
        [ProducesResponseType(typeof(ApiResponseBase), 403)]
        [ProducesResponseType(typeof(ApiResponseBase), 404)]
        [ProducesResponseType(typeof(ApiResponseBase), 400)]
        [ProducesResponseType(typeof(ApiResponseBase), 500)]
        public async Task<ActionResult<ApiResponseBase>> SetComputerMaintenance(int id, [FromBody] string reason)
        {
            try
            {
                await _computerService.SetComputerMaintenanceAsync(id, reason);
                return Ok(ApiResponseFactory.Success("Đặt máy tính ở chế độ bảo trì thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đặt máy tính có ID {ComputerId} ở chế độ bảo trì", id);

                if (ex.Message.Contains("not found"))
                    return NotFound(ApiResponseFactory.Fail($"Không tìm thấy máy tính có ID {id}"));
                else if (ex.Message.Contains("active session"))
                    return BadRequest(ApiResponseFactory.Fail(ex.Message));

                return StatusCode(500, ApiResponseFactory.Fail("Lỗi server khi đặt máy tính ở chế độ bảo trì"));
            }
        }

        [HttpGet("{id}/is-available")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 401)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 500)]
        public async Task<ActionResult<ApiResponse<bool>>> IsComputerAvailable(int id)
        {
            try
            {
                var isAvailable = await _computerService.IsComputerAvailableAsync(id);
                return Ok(ApiResponseFactory.Success(isAvailable, "Kiểm tra trạng thái máy tính thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra trạng thái khả dụng của máy tính có ID {ComputerId}", id);

                if (ex.Message.Contains("not found"))
                    return NotFound(ApiResponseFactory.Fail<bool>($"Không tìm thấy máy tính có ID {id}"));

                return StatusCode(500, ApiResponseFactory.Fail<bool>("Lỗi server khi kiểm tra trạng thái máy tính"));
            }
        }
    }
}