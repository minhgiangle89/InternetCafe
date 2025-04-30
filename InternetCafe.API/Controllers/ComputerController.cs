using InternetCafe.Application.DTOs.Computer;
using InternetCafe.Application.Interfaces.Services;
using InternetCafe.Domain.Enums;
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
        [ProducesResponseType(typeof(IEnumerable<ComputerDTO>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<IEnumerable<ComputerDTO>>> GetAllComputers()
        {
            try
            {
                var computers = await _computerService.GetAllComputersAsync();
                return Ok(computers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all computers");
                return StatusCode(500, new { Message = "An error occurred while retrieving computers" });
            }
        }

        [HttpGet("available")]
        [ProducesResponseType(typeof(IEnumerable<ComputerDTO>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<IEnumerable<ComputerDTO>>> GetAvailableComputers()
        {
            try
            {
                var computers = await _computerService.GetAvailableComputersAsync();
                return Ok(computers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available computers");
                return StatusCode(500, new { Message = "An error occurred while retrieving available computers" });
            }
        }

        [HttpGet("status/{status}")]
        [ProducesResponseType(typeof(IEnumerable<ComputerDTO>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<IEnumerable<ComputerDTO>>> GetComputersByStatus(int status)
        {
            try
            {
                if (!Enum.IsDefined(typeof(ComputerStatus), status))
                {
                    return BadRequest(new { Message = "Invalid status value" });
                }

                var computers = await _computerService.GetComputersByStatusAsync((ComputerStatus)status);
                return Ok(computers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error retrieving computers with status {0}", status));
                return StatusCode(500, new { Message = "An error occurred while retrieving computers" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ComputerDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ComputerDTO>> GetComputerById(int id)
        {
            try
            {
                var computer = await _computerService.GetComputerByIdAsync(id);
                return Ok(computer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error retrieving computer with ID {0}", id));
                return ex.Message.Contains("not found") ? NotFound(new { Message = ex.Message }) :
                    StatusCode(500, new { Message = "An error occurred while retrieving computer" });
            }
        }

        [HttpGet("{id}/details")]
        [ProducesResponseType(typeof(ComputerDetailsDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ComputerDetailsDTO>> GetComputerDetails(int id)
        {
            try
            {
                var computerDetails = await _computerService.GetComputerDetailsAsync(id);
                return Ok(computerDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error retrieving computer details for computer with ID {0}", id));
                return ex.Message.Contains("not found") ? NotFound(new { Message = ex.Message }) :
                    StatusCode(500, new { Message = "An error occurred while retrieving computer details" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "1,2")] // Staff and Admin only
        [ProducesResponseType(typeof(ComputerDTO), 201)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ComputerDTO>> RegisterComputer([FromBody] CreateComputerDTO createComputerDTO)
        {
            try
            {
                var computer = await _computerService.RegisterComputerAsync(createComputerDTO);
                return CreatedAtAction(nameof(GetComputerById), new { id = computer.Id }, computer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error registering computer with name {0}", createComputerDTO.Name));
                return ex.Message.Contains("already exists") || ex.Message.Contains("Invalid") ?
                    BadRequest(new { Message = ex.Message }) :
                    StatusCode(500, new { Message = "An error occurred while registering computer" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "1,2")] // Staff and Admin only
        [ProducesResponseType(typeof(ComputerDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ComputerDTO>> UpdateComputer(int id, [FromBody] UpdateComputerDTO updateComputerDTO)
        {
            try
            {
                var computer = await _computerService.UpdateComputerAsync(id, updateComputerDTO);
                return Ok(computer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error updating computer with ID {0}", id));
                if (ex.Message.Contains("not found"))
                    return NotFound(new { Message = ex.Message });
                else if (ex.Message.Contains("already exists") || ex.Message.Contains("Invalid"))
                    return BadRequest(new { Message = ex.Message });
                else
                    return StatusCode(500, new { Message = "An error occurred while updating computer" });
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "1,2")] // Staff and Admin only
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> UpdateComputerStatus(int id, [FromBody] ComputerStatusUpdateDTO updateDTO)
        {
            try
            {
                if (!Enum.IsDefined(typeof(ComputerStatus), updateDTO.Status))
                {
                    return BadRequest(new { Message = "Invalid status value" });
                }

                // Ensure computer ID in path matches ID in body
                if (id != updateDTO.ComputerId)
                {
                    updateDTO.ComputerId = id;
                }

                await _computerService.SetComputerStatusAsync(updateDTO);
                return Ok(new { Message = "Computer status updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error updating status for computer with ID {0}", id));
                if (ex.Message.Contains("not found"))
                    return NotFound(new { Message = ex.Message });
                else if (ex.Message.Contains("active session"))
                    return BadRequest(new { Message = ex.Message });
                else
                    return StatusCode(500, new { Message = "An error occurred while updating computer status" });
            }
        }

        [HttpPost("{id}/maintenance")]
        [Authorize(Roles = "1,2")] // Staff and Admin only
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> SetComputerMaintenance(int id, [FromBody] string reason)
        {
            try
            {
                await _computerService.SetComputerMaintenanceAsync(id, reason);
                return Ok(new { Message = "Computer set to maintenance mode" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error setting maintenance mode for computer with ID {0}", id));
                if (ex.Message.Contains("not found"))
                    return NotFound(new { Message = ex.Message });
                else if (ex.Message.Contains("active session"))
                    return BadRequest(new { Message = ex.Message });
                else
                    return StatusCode(500, new { Message = "An error occurred while setting maintenance mode" });
            }
        }

        [HttpGet("{id}/is-available")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<bool>> IsComputerAvailable(int id)
        {
            try
            {
                var isAvailable = await _computerService.IsComputerAvailableAsync(id);
                return Ok(isAvailable);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error checking availability for computer with ID {0}", id));
                return ex.Message.Contains("not found") ? NotFound(new { Message = ex.Message }) :
                    StatusCode(500, new { Message = "An error occurred while checking computer availability" });
            }
        }
    }
}