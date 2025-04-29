using InternetCafe.Application.DTOs.User;
using InternetCafe.Domain.Entities;
using InternetCafe.Domain.Enums;
using InternetCafe.Domain.Exceptions;
using InternetCafe.Domain.ValueObjects;
using InternetCafe.Application.Interfaces;
using InternetCafe.Application.Interfaces.Repositories;
using InternetCafe.Application.Interfaces.Services;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Authentication;

namespace InternetCafe.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMapper _mapper;
        private readonly IAuditLogger _auditLogger;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IMapper mapper,
            IAuditLogger auditLogger,
            ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UserDTO> AuthenticateUserAsync(string username, string password)
        {
            var user = await _unitOfWork.Users.GetByUsernameAsync(username);
            if (user == null)
            {
                throw new UserNotFoundException(username);
            }

            if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
            {
                throw new AuthenticationException("Invalid password");
            }

            // Update last login time
            user.LastLoginTime = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO> RegisterUserAsync(CreateUserDTO userDTO)
        {
            try
            {
                // Validate if username or email already exists
                var existingUserWithUsername = await _unitOfWork.Users.GetByUsernameAsync(userDTO.Username);
                if (existingUserWithUsername != null)
                {
                    throw new DuplicateUserException("username", userDTO.Username);
                }

                var existingUserWithEmail = await _unitOfWork.Users.GetByEmailAsync(userDTO.Email);
                if (existingUserWithEmail != null)
                {
                    throw new DuplicateUserException("email", userDTO.Email);
                }

                // Create new user
                var user = new User
                {
                    Username = userDTO.Username,
                    Email = userDTO.Email,
                    PasswordHash = _passwordHasher.HashPassword(userDTO.Password),
                    FullName = userDTO.FullName,
                    PhoneNumber = userDTO.PhoneNumber,
                    Address = userDTO.Address,
                    DateOfBirth = userDTO.DateOfBirth,
                    Role = userDTO.Role,
                    LastLoginTime = DateTime.UtcNow,
                    Status = (int)UserStatus.Active
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.CompleteAsync();

                // Log user creation
                await _auditLogger.LogActivityAsync(
                    "UserCreated",
                    "User",
                    user.Id,
                    0, // System operation
                    DateTime.UtcNow,
                    $"User {user.Username} was created");

                return _mapper.Map<UserDTO>(user);
            }
            catch (Exception ex) when (ex is not DuplicateUserException)
            {
                _logger.LogError(ex, "Error registering user: {Username}", userDTO.Username);
                throw;
            }
        }

        public async Task UpdateUserAsync(int userId, UpdateUserDTO userDTO)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            // Update user properties
            user.Email = userDTO.Email;
            user.FullName = userDTO.FullName;
            user.PhoneNumber = userDTO.PhoneNumber;
            user.Address = userDTO.Address;
            user.DateOfBirth = userDTO.DateOfBirth;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            // Log user update
            await _auditLogger.LogActivityAsync(
                "UserUpdated",
                "User",
                userId,
                userId,
                DateTime.UtcNow,
                $"User {user.Username} was updated");
        }

        public async Task<bool> CheckPasswordAsync(int userId, string password)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            return _passwordHasher.VerifyPassword(password, user.PasswordHash);
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordDTO changePasswordDTO)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            // Verify current password
            if (!_passwordHasher.VerifyPassword(changePasswordDTO.CurrentPassword, user.PasswordHash))
            {
                throw new AuthenticationException("Current password is incorrect");
            }

            // Update password
            user.PasswordHash = _passwordHasher.HashPassword(changePasswordDTO.NewPassword);
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            // Log password change
            await _auditLogger.LogActivityAsync(
                "PasswordChanged",
                "User",
                userId,
                userId,
                DateTime.UtcNow,
                "Password was changed");
        }

        public async Task ChangeUserStatusAsync(int userId, UserStatus status)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            user.Status = (int)status;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            // Log status change
            await _auditLogger.LogActivityAsync(
                "UserStatusChanged",
                "User",
                userId,
                userId,
                DateTime.UtcNow,
                $"User status changed to {status}");
        }

        public async Task<UserDTO> GetUserByIdAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<UserDetailsDTO> GetUserDetailsAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetUserWithAccountAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            var userDetails = _mapper.Map<UserDetailsDTO>(user);

            // Get recent sessions
            var sessions = await _unitOfWork.Sessions.GetByUserIdAsync(userId);
            userDetails.RecentSessions = _mapper.Map<ICollection<DTOs.Session.SessionSummaryDTO>>(sessions);

            return userDetails;
        }

        public async Task<UserDTO> GetUserByUsernameAsync(string username)
        {
            var user = await _unitOfWork.Users.GetByUsernameAsync(username);
            if (user == null)
            {
                throw new UserNotFoundException(username);
            }

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDTO>>(users);
        }
    }
}