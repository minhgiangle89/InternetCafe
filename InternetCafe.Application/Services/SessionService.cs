using InternetCafe.Application.DTOs.Session;
using InternetCafe.Application.Interfaces;
using InternetCafe.Application.Interfaces.Services;
using InternetCafe.Domain.Entities;
using InternetCafe.Domain.Enums;
using InternetCafe.Domain.Exceptions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InternetCafe.Application.Services
{
    public class SessionService : ISessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAccountService _accountService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditLogger _auditLogger;
        private readonly ILogger<SessionService> _logger;

        public SessionService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IAccountService accountService,
            ICurrentUserService currentUserService,
            IAuditLogger auditLogger,
            ILogger<SessionService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SessionDTO> StartSessionAsync(StartSessionDTO startSessionDTO)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Validate user exists
                var user = await _unitOfWork.Users.GetByIdAsync(startSessionDTO.UserId);
                if (user == null)
                {
                    throw new UserNotFoundException(startSessionDTO.UserId);
                }

                // Check if user already has an active session
                var existingUserSession = await HasActiveSessionAsync(startSessionDTO.UserId);
                if (existingUserSession)
                {
                    throw new Exception($"User with ID {startSessionDTO.UserId} already has an active session.");
                }

                // Validate computer exists and is available
                var computer = await _unitOfWork.Computers.GetByIdAsync(startSessionDTO.ComputerId);
                if (computer == null)
                {
                    throw new Exception($"Computer with ID {startSessionDTO.ComputerId} not found.");
                }

                if (computer.ComputerStatus != (int)ComputerStatus.Available)
                {
                    throw new ComputerNotAvailableException(startSessionDTO.ComputerId);
                }

                // Check if user has sufficient balance
                var account = await _unitOfWork.Accounts.GetByUserIdAsync(startSessionDTO.UserId);
                if (account == null)
                {
                    throw new Exception($"Account for user with ID {startSessionDTO.UserId} not found.");
                }

                // Minimum balance check (at least 15 minutes of usage)
                decimal minimumBalance = computer.HourlyRate / 4;
                if (account.Balance < minimumBalance)
                {
                    throw new InsufficientBalanceException(account.Balance, minimumBalance);
                }

                // Create session
                var session = new Session
                {
                    UserId = startSessionDTO.UserId,
                    ComputerId = startSessionDTO.ComputerId,
                    StartTime = DateTime.UtcNow,
                    Duration = TimeSpan.Zero,
                    TotalCost = 0,
                    Status = SessionStatus.Active
                };

                await _unitOfWork.Sessions.AddAsync(session);

                // Update computer status
                await _unitOfWork.Computers.UpdateStatusAsync(startSessionDTO.ComputerId, ComputerStatus.InUse);

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Log session start
                await _auditLogger.LogActivityAsync(
                    "SessionStarted",
                    "Session",
                    session.Id,
                    _currentUserService.UserId ?? startSessionDTO.UserId,
                    DateTime.UtcNow,
                    $"Session started for user {startSessionDTO.UserId} on computer {startSessionDTO.ComputerId}");

                var sessionDto = _mapper.Map<SessionDTO>(session);
                sessionDto.UserName = user.Username;
                sessionDto.ComputerName = computer.Name;

                return sessionDto;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error starting session for user {UserId} on computer {ComputerId}",
                    startSessionDTO.UserId, startSessionDTO.ComputerId);
                throw;
            }
        }

        public async Task<SessionDTO> EndSessionAsync(EndSessionDTO endSessionDTO)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Validate session exists and is active
                var session = await _unitOfWork.Sessions.GetByIdAsync(endSessionDTO.SessionId);
                if (session == null)
                {
                    throw new SessionNotFoundException(endSessionDTO.SessionId);
                }

                if (session.Status != SessionStatus.Active)
                {
                    throw new Exception($"Session with ID {endSessionDTO.SessionId} is not active.");
                }

                // Get computer details
                var computer = await _unitOfWork.Computers.GetByIdAsync(session.ComputerId);
                if (computer == null)
                {
                    throw new Exception($"Computer with ID {session.ComputerId} not found.");
                }

                // Calculate session duration and cost
                var endTime = DateTime.UtcNow;
                var duration = endTime - session.StartTime;
                var cost = CalculateSessionCost(duration, computer.HourlyRate);

                // Update session
                session.EndTime = endTime;
                session.Duration = duration;
                session.TotalCost = cost;
                session.Status = SessionStatus.Completed;
                session.Notes = endSessionDTO.Notes;

                await _unitOfWork.Sessions.UpdateAsync(session);

                // Update computer status
                await _unitOfWork.Computers.UpdateStatusAsync(session.ComputerId, ComputerStatus.Available);

                // Update account balance
                var account = await _unitOfWork.Accounts.GetByUserIdAsync(session.UserId);
                if (account == null)
                {
                    throw new Exception($"Account for user with ID {session.UserId} not found.");
                }

                // Create charge transaction
                await _accountService.ChargeForSessionAsync(account.Id, session.Id, cost);

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Log session end
                await _auditLogger.LogActivityAsync(
                    "SessionEnded",
                    "Session",
                    session.Id,
                    _currentUserService.UserId ?? session.UserId,
                    DateTime.UtcNow,
                    $"Session ended for user {session.UserId}. Duration: {duration}, Cost: {cost}");

                var sessionDto = _mapper.Map<SessionDTO>(session);
                var user = await _unitOfWork.Users.GetByIdAsync(session.UserId);
                sessionDto.UserName = user?.Username ?? "Unknown";
                sessionDto.ComputerName = computer.Name;

                return sessionDto;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error ending session {SessionId}", endSessionDTO.SessionId);
                throw;
            }
        }

        public async Task<SessionDTO> GetActiveSessionByComputerIdAsync(int computerId)
        {
            var session = await _unitOfWork.Sessions.GetCurrentSessionByComputerIdAsync(computerId);
            if (session == null)
            {
                return null;
            }

            var sessionDto = _mapper.Map<SessionDTO>(session);

            // Load related data
            var computer = await _unitOfWork.Computers.GetByIdAsync(computerId);
            var user = await _unitOfWork.Users.GetByIdAsync(session.UserId);

            sessionDto.ComputerName = computer?.Name ?? "Unknown";
            sessionDto.UserName = user?.Username ?? "Unknown";

            return sessionDto;
        }

        public async Task<IEnumerable<SessionDTO>> GetActiveSessionsAsync()
        {
            var sessions = await _unitOfWork.Sessions.GetActiveSessionsAsync();
            var sessionDtos = _mapper.Map<IEnumerable<SessionDTO>>(sessions).ToList();

            // Load related data
            foreach (var sessionDto in sessionDtos)
            {
                var computer = await _unitOfWork.Computers.GetByIdAsync(sessionDto.ComputerId);
                var user = await _unitOfWork.Users.GetByIdAsync(sessionDto.UserId);

                sessionDto.ComputerName = computer?.Name ?? "Unknown";
                sessionDto.UserName = user?.Username ?? "Unknown";
            }

            return sessionDtos;
        }

        public async Task<IEnumerable<SessionDTO>> GetSessionsByUserIdAsync(int userId)
        {
            var sessions = await _unitOfWork.Sessions.GetByUserIdAsync(userId);
            var sessionDtos = _mapper.Map<IEnumerable<SessionDTO>>(sessions).ToList();

            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            // Load related data
            foreach (var sessionDto in sessionDtos)
            {
                var computer = await _unitOfWork.Computers.GetByIdAsync(sessionDto.ComputerId);
                sessionDto.ComputerName = computer?.Name ?? "Unknown";
                sessionDto.UserName = user?.Username ?? "Unknown";
            }

            return sessionDtos;
        }

        public async Task<IEnumerable<SessionDTO>> GetSessionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var sessions = await _unitOfWork.Sessions.FindAsync(s =>
                s.StartTime >= startDate &&
                (s.EndTime == null || s.EndTime <= endDate));

            var sessionDtos = _mapper.Map<IEnumerable<SessionDTO>>(sessions).ToList();

            // Load related data
            foreach (var sessionDto in sessionDtos)
            {
                var computer = await _unitOfWork.Computers.GetByIdAsync(sessionDto.ComputerId);
                var user = await _unitOfWork.Users.GetByIdAsync(sessionDto.UserId);

                sessionDto.ComputerName = computer?.Name ?? "Unknown";
                sessionDto.UserName = user?.Username ?? "Unknown";
            }

            return sessionDtos;
        }

        public async Task<decimal> CalculateSessionCostAsync(int sessionId)
        {
            var session = await _unitOfWork.Sessions.GetByIdAsync(sessionId);
            if (session == null)
            {
                throw new SessionNotFoundException(sessionId);
            }

            if (session.EndTime == null)
            {
                // For active sessions, calculate current cost
                var computer = await _unitOfWork.Computers.GetByIdAsync(session.ComputerId);
                if (computer == null)
                {
                    throw new Exception($"Computer with ID {session.ComputerId} not found.");
                }

                var currentDuration = DateTime.UtcNow - session.StartTime;
                return CalculateSessionCost(currentDuration, computer.HourlyRate);
            }

            return session.TotalCost;
        }

        public async Task<double> CalculateSessionCostAsync(TimeSpan duration, double hourlyRate)
        {
            return Math.Ceiling(duration.TotalHours * hourlyRate);
        }

        private decimal CalculateSessionCost(TimeSpan duration, decimal hourlyRate)
        {
            // Round up to the next minute
            double totalHours = Math.Ceiling(duration.TotalMinutes) / 60;
            return Math.Round(Convert.ToDecimal(totalHours) * hourlyRate, 2);
        }

        public async Task<TimeSpan> GetRemainingTimeAsync(int userId, int computerId)
        {
            // Get active session
            var session = await _unitOfWork.Sessions.GetCurrentSessionByComputerIdAsync(computerId);
            if (session == null || session.UserId != userId)
            {
                throw new Exception("No active session found for this user and computer.");
            }

            // Get account and computer
            var account = await _unitOfWork.Accounts.GetByUserIdAsync(userId);
            if (account == null)
            {
                throw new Exception($"Account for user with ID {userId} not found.");
            }

            var computer = await _unitOfWork.Computers.GetByIdAsync(computerId);
            if (computer == null)
            {
                throw new Exception($"Computer with ID {computerId} not found.");
            }

            // Calculate current session cost
            var currentDuration = DateTime.UtcNow - session.StartTime;
            var currentCost = CalculateSessionCost(currentDuration, computer.HourlyRate);

            // Calculate remaining time based on available balance
            var remainingBalance = account.Balance;
            var hourlyRate = computer.HourlyRate;

            // If hourly rate is zero (e.g., free usage), return a large timespan
            if (hourlyRate <= 0)
            {
                return TimeSpan.FromDays(365); // Return a large time span
            }

            var remainingHours = remainingBalance / hourlyRate;
            return TimeSpan.FromHours(Convert.ToDouble(remainingHours));
        }

        public async Task<bool> HasActiveSessionAsync(int userId)
        {
            var sessions = await _unitOfWork.Sessions.FindAsync(s =>
                s.UserId == userId && s.Status == SessionStatus.Active);

            return sessions.Any();
        }

        public async Task<SessionDTO> TerminateSessionAsync(int sessionId, string reason)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Validate session exists and is active
                var session = await _unitOfWork.Sessions.GetByIdAsync(sessionId);
                if (session == null)
                {
                    throw new SessionNotFoundException(sessionId);
                }

                if (session.Status != SessionStatus.Active)
                {
                    throw new Exception($"Session with ID {sessionId} is not active.");
                }

                // Get computer details
                var computer = await _unitOfWork.Computers.GetByIdAsync(session.ComputerId);
                if (computer == null)
                {
                    throw new Exception($"Computer with ID {session.ComputerId} not found.");
                }

                // Calculate session duration and cost
                var endTime = DateTime.UtcNow;
                var duration = endTime - session.StartTime;
                var cost = CalculateSessionCost(duration, computer.HourlyRate);

                // Update session
                session.EndTime = endTime;
                session.Duration = duration;
                session.TotalCost = cost;
                session.Status = SessionStatus.Terminated;
                session.Notes = reason;

                await _unitOfWork.Sessions.UpdateAsync(session);

                // Update computer status
                await _unitOfWork.Computers.UpdateStatusAsync(session.ComputerId, ComputerStatus.Available);

                // Update account balance
                var account = await _unitOfWork.Accounts.GetByUserIdAsync(session.UserId);
                if (account == null)
                {
                    throw new Exception($"Account for user with ID {session.UserId} not found.");
                }

                // Create charge transaction
                await _accountService.ChargeForSessionAsync(account.Id, session.Id, cost);

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Log session termination
                await _auditLogger.LogActivityAsync(
                    "SessionTerminated",
                    "Session",
                    session.Id,
                    _currentUserService.UserId ?? session.UserId,
                    DateTime.UtcNow,
                    $"Session terminated. Reason: {reason}. Duration: {duration}, Cost: {cost}");

                var sessionDto = _mapper.Map<SessionDTO>(session);
                var user = await _unitOfWork.Users.GetByIdAsync(session.UserId);
                sessionDto.UserName = user?.Username ?? "Unknown";
                sessionDto.ComputerName = computer.Name;

                return sessionDto;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error terminating session {SessionId}", sessionId);
                throw;
            }
        }

        public async Task<SessionDetailsDTO> GetSessionDetailsAsync(int sessionId)
        {
            var session = await _unitOfWork.Sessions.GetByIdAsync(sessionId);
            if (session == null)
            {
                throw new SessionNotFoundException(sessionId);
            }

            var sessionDetailsDto = _mapper.Map<SessionDetailsDTO>(session);

            // Load related data
            var computer = await _unitOfWork.Computers.GetByIdAsync(session.ComputerId);
            var user = await _unitOfWork.Users.GetByIdAsync(session.UserId);

            sessionDetailsDto.ComputerName = computer?.Name ?? "Unknown";
            sessionDetailsDto.UserName = user?.Username ?? "Unknown";

            // Load transactions
            var transactions = await _unitOfWork.Transactions.GetBySessionIdAsync(sessionId);
            sessionDetailsDto.Transactions = _mapper.Map<ICollection<DTOs.Transaction.TransactionDTO>>(transactions);

            // Add usernames to transactions
            foreach (var transaction in sessionDetailsDto.Transactions)
            {
                if (transaction.UserId.HasValue)
                {
                    var transactionUser = await _unitOfWork.Users.GetByIdAsync(transaction.UserId.Value);
                    transaction.UserName = transactionUser?.Username;
                }
            }

            return sessionDetailsDto;
        }
    }
}