using InternetCafe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace InternetCafe.Infrastructure.Logging
{
    public class AuditLogger : IAuditLogger
    {
        private readonly DbContext _dbContext;
        private readonly ILogger<AuditLogger> _logger;

        public AuditLogger(DbContext dbContext, ILogger<AuditLogger> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task LogActivityAsync(string action, string entityName, int entityId, int userId, DateTime timestamp, string? details = null)
        {
            try
            {
                var audit = new AuditLog
                {
                    Action = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    UserId = userId,
                    Timestamp = timestamp,
                    Details = details
                };

                await _dbContext.Set<AuditLog>().AddAsync(audit);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Audit log created for {Action} on {EntityName} with ID {EntityId} by user {UserId}",
                    action, entityName, entityId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create audit log for {Action} on {EntityName} with ID {EntityId}",
                    action, entityName, entityId);
            }
        }

        public async Task LogLoginAttemptAsync(string username, bool success, string ipAddress, DateTime timestamp)
        {
            try
            {
                var loginAttempt = new LoginAttempt
                {
                    Username = username,
                    Success = success,
                    IpAddress = ipAddress,
                    Timestamp = timestamp
                };

                await _dbContext.Set<LoginAttempt>().AddAsync(loginAttempt);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Login attempt logged for user {Username} from IP {IpAddress}. Success: {Success}",
                    username, ipAddress, success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log login attempt for user {Username}", username);
            }
        }

        public async Task LogSystemEventAsync(string eventType, string description, DateTime timestamp, int? userId = null)
        {
            try
            {
                var systemEvent = new SystemEvent
                {
                    EventType = eventType,
                    Description = description,
                    Timestamp = timestamp,
                    UserId = userId
                };

                await _dbContext.Set<SystemEvent>().AddAsync(systemEvent);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("System event logged: {EventType} - {Description}", eventType, description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log system event: {EventType}", eventType);
            }
        }
    }

    // Entities for audit logging
    public class AuditLog
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public int UserId { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Details { get; set; }
    }

    public class LoginAttempt
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class SystemEvent
    {
        public int Id { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public int? UserId { get; set; }
    }
}