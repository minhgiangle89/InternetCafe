using InternetCafe.Application.DTOs.Computer;
using InternetCafe.Application.Interfaces;
using InternetCafe.Application.Interfaces.Services;
using InternetCafe.Domain.Entities;
using InternetCafe.Domain.Enums;
using InternetCafe.Domain.Exceptions;
using InternetCafe.Domain.ValueObjects;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InternetCafe.Application.Services
{
    public class ComputerService : IComputerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditLogger _auditLogger;
        private readonly ILogger<ComputerService> _logger;

        public ComputerService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IAuditLogger auditLogger,
            ILogger<ComputerService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ComputerDTO> RegisterComputerAsync(CreateComputerDTO computerDTO)
        {
            try
            {
                // Validate IP Address
                try
                {
                    var ipAddressValue = new IPAddressValue(computerDTO.IPAddress);
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException($"Invalid IP address: {ex.Message}");
                }

                // Check if computer with same name or IP already exists
                var existingComputers = await _unitOfWork.Computers.FindAsync(c =>
                    c.Name == computerDTO.Name || c.IPAddress == computerDTO.IPAddress);

                if (existingComputers.Any(c => c.Name == computerDTO.Name))
                {
                    throw new Exception($"Computer with name '{computerDTO.Name}' already exists.");
                }

                if (existingComputers.Any(c => c.IPAddress == computerDTO.IPAddress))
                {
                    throw new Exception($"Computer with IP address '{computerDTO.IPAddress}' already exists.");
                }

                // Create computer
                var computer = new Computer
                {
                    Name = computerDTO.Name,
                    IPAddress = computerDTO.IPAddress,
                    Specifications = computerDTO.Specifications,
                    Location = computerDTO.Location,
                    ComputerStatus = (int)ComputerStatus.Available,
                    HourlyRate = computerDTO.HourlyRate,
                    LastMaintenanceDate = DateTime.UtcNow
                };

                await _unitOfWork.Computers.AddAsync(computer);
                await _unitOfWork.CompleteAsync();

                // Log computer creation
                await _auditLogger.LogActivityAsync(
                    "ComputerCreated",
                    "Computer",
                    computer.Id,
                    _currentUserService.UserId ?? 0,
                    DateTime.UtcNow,
                    $"Computer {computer.Name} was registered");

                return _mapper.Map<ComputerDTO>(computer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering computer with name {ComputerName}", computerDTO.Name);
                throw;
            }
        }

        public async Task<IEnumerable<ComputerDTO>> GetAvailableComputersAsync()
        {
            var computers = await _unitOfWork.Computers.GetAvailableComputersAsync();
            return _mapper.Map<IEnumerable<ComputerDTO>>(computers);
        }

        public async Task<ComputerDTO> GetComputerByIdAsync(int computerId)
        {
            var computer = await _unitOfWork.Computers.GetByIdAsync(computerId);
            if (computer == null)
            {
                throw new Exception($"Computer with ID {computerId} not found.");
            }

            return _mapper.Map<ComputerDTO>(computer);
        }

        public async Task<ComputerDetailsDTO> GetComputerDetailsAsync(int computerId)
        {
            var computer = await _unitOfWork.Computers.GetByIdAsync(computerId);
            if (computer == null)
            {
                throw new Exception($"Computer with ID {computerId} not found.");
            }

            var computerDetailsDto = _mapper.Map<ComputerDetailsDTO>(computer);

            // Get current session if any
            var currentSession = await _unitOfWork.Sessions.GetCurrentSessionByComputerIdAsync(computerId);
            if (currentSession != null)
            {
                var sessionDto = _mapper.Map<DTOs.Session.SessionDTO>(currentSession);
                var user = await _unitOfWork.Users.GetByIdAsync(currentSession.UserId);
                sessionDto.UserName = user?.Username ?? "Unknown";
                sessionDto.ComputerName = computer.Name;

                computerDetailsDto.CurrentSession = sessionDto;
            }

            // Get recent sessions (last 5)
            var recentSessions = await _unitOfWork.Sessions.GetByComputerIdAsync(computerId);
            computerDetailsDto.RecentSessions = _mapper.Map<ICollection<DTOs.Session.SessionSummaryDTO>>(
                recentSessions.Take(5).ToList());

            return computerDetailsDto;
        }

        public async Task SetComputerStatusAsync(ComputerStatusUpdateDTO updateDTO)
        {
            try
            {
                var computer = await _unitOfWork.Computers.GetByIdAsync(updateDTO.ComputerId);
                if (computer == null)
                {
                    throw new Exception($"Computer with ID {updateDTO.ComputerId} not found.");
                }

                // Check if computer has active session when trying to set to maintenance or out of order
                var newStatus = (ComputerStatus)updateDTO.Status;
                if ((newStatus == ComputerStatus.Maintenance || newStatus == ComputerStatus.OutOfOrder)
                    && computer.ComputerStatus == (int)ComputerStatus.InUse)
                {
                    var activeSession = await _unitOfWork.Sessions.GetCurrentSessionByComputerIdAsync(updateDTO.ComputerId);
                    if (activeSession != null)
                    {
                        throw new Exception($"Cannot change status of computer with active session. End the session first.");
                    }
                }

                await _unitOfWork.Computers.UpdateStatusAsync(updateDTO.ComputerId, (ComputerStatus)updateDTO.Status);
                await _unitOfWork.CompleteAsync();

                // If setting to maintenance mode, update maintenance date
                if (newStatus == ComputerStatus.Maintenance)
                {
                    computer.LastMaintenanceDate = DateTime.UtcNow;
                    await _unitOfWork.Computers.UpdateAsync(computer);
                    await _unitOfWork.CompleteAsync();
                }

                // Log status change
                await _auditLogger.LogActivityAsync(
                    "ComputerStatusChanged",
                    "Computer",
                    computer.Id,
                    _currentUserService.UserId ?? 0,
                    DateTime.UtcNow,
                    $"Computer {computer.Name} status changed to {newStatus}. Reason: {updateDTO.Reason}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing computer status for computer {ComputerId}", updateDTO.ComputerId);
                throw;
            }
        }

        public async Task<ComputerDTO> UpdateComputerAsync(int computerId, UpdateComputerDTO computerDTO)
        {
            try
            {
                var computer = await _unitOfWork.Computers.GetByIdAsync(computerId);
                if (computer == null)
                {
                    throw new Exception($"Computer with ID {computerId} not found.");
                }

                // Validate IP Address
                try
                {
                    var ipAddressValue = new IPAddressValue(computerDTO.IPAddress);
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException($"Invalid IP address: {ex.Message}");
                }

                // If name or IP address has changed, check for duplicates
                if (computer.Name != computerDTO.Name || computer.IPAddress != computerDTO.IPAddress)
                {
                    var existingComputers = await _unitOfWork.Computers.FindAsync(c =>
                        (c.Name == computerDTO.Name || c.IPAddress == computerDTO.IPAddress) && c.Id != computerId);

                    if (existingComputers.Any(c => c.Name == computerDTO.Name))
                    {
                        throw new Exception($"Computer with name '{computerDTO.Name}' already exists.");
                    }

                    if (existingComputers.Any(c => c.IPAddress == computerDTO.IPAddress))
                    {
                        throw new Exception($"Computer with IP address '{computerDTO.IPAddress}' already exists.");
                    }
                }

                // Update properties
                computer.Name = computerDTO.Name;
                computer.IPAddress = computerDTO.IPAddress;
                computer.Specifications = computerDTO.Specifications;
                computer.Location = computerDTO.Location;
                computer.HourlyRate = computerDTO.HourlyRate;

                await _unitOfWork.Computers.UpdateAsync(computer);
                await _unitOfWork.CompleteAsync();

                // Log computer update
                await _auditLogger.LogActivityAsync(
                    "ComputerUpdated",
                    "Computer",
                    computer.Id,
                    _currentUserService.UserId ?? 0,
                    DateTime.UtcNow,
                    $"Computer {computer.Name} was updated");

                return _mapper.Map<ComputerDTO>(computer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating computer with ID {ComputerId}", computerId);
                throw;
            }
        }

        public async Task<bool> IsComputerAvailableAsync(int computerId)
        {
            var computer = await _unitOfWork.Computers.GetByIdAsync(computerId);
            if (computer == null)
            {
                throw new Exception($"Computer with ID {computerId} not found.");
            }

            return computer.ComputerStatus == (int)ComputerStatus.Available;
        }

        public async Task SetComputerMaintenanceAsync(int computerId, string reason)
        {
            try
            {
                var computer = await _unitOfWork.Computers.GetByIdAsync(computerId);
                if (computer == null)
                {
                    throw new Exception($"Computer with ID {computerId} not found.");
                }

                // Check if computer has active session
                if (computer.ComputerStatus == (int)ComputerStatus.InUse)
                {
                    var activeSession = await _unitOfWork.Sessions.GetCurrentSessionByComputerIdAsync(computerId);
                    if (activeSession != null)
                    {
                        throw new Exception($"Cannot set computer to maintenance mode with active session. End the session first.");
                    }
                }

                // Update computer status
                await _unitOfWork.Computers.UpdateStatusAsync(computerId, ComputerStatus.Maintenance);

                // Update maintenance date
                computer.LastMaintenanceDate = DateTime.UtcNow;
                await _unitOfWork.Computers.UpdateAsync(computer);
                await _unitOfWork.CompleteAsync();

                // Log maintenance
                await _auditLogger.LogActivityAsync(
                    "ComputerMaintenance",
                    "Computer",
                    computer.Id,
                    _currentUserService.UserId ?? 0,
                    DateTime.UtcNow,
                    $"Computer {computer.Name} set to maintenance mode. Reason: {reason}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting computer to maintenance mode for computer {ComputerId}", computerId);
                throw;
            }
        }

        public async Task<IEnumerable<ComputerDTO>> GetComputersByStatusAsync(ComputerStatus status)
        {
            var computers = await _unitOfWork.Computers.GetByStatusAsync(status);
            return _mapper.Map<IEnumerable<ComputerDTO>>(computers);
        }

        public async Task<IEnumerable<ComputerDTO>> GetAllComputersAsync()
        {
            var computers = await _unitOfWork.Computers.GetAllAsync();
            return _mapper.Map<IEnumerable<ComputerDTO>>(computers);
        }
    }
}