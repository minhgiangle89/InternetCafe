using InternetCafe.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace InternetCafe.Infrastructure.Logging
{
    public class AuditLogger : IAuditLogger
    {
        private readonly ILogger<AuditLogger> _logger;

        public AuditLogger(ILogger<AuditLogger> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task LogActivityAsync(string action, string entityName, int entityId, int userId, DateTime timestamp, string? details = null)
        {
            _logger.LogInformation(
                "AUDIT: Action={Action}, Entity={EntityName}, EntityId={EntityId}, UserId={UserId}, Timestamp={Timestamp}, Details={Details}",
                action, entityName, entityId, userId, timestamp, details);

            return Task.CompletedTask;
        }

        public Task LogLoginAttemptAsync(string username, bool success, string ipAddress, DateTime timestamp)
        {
            if (success)
            {
                _logger.LogInformation(
                    "LOGIN: User={Username} successfully logged in from IP={IpAddress} at {Timestamp}",
                    username, ipAddress, timestamp);
            }
            else
            {
                _logger.LogWarning(
                    "LOGIN FAILED: User={Username} failed to log in from IP={IpAddress} at {Timestamp}",
                    username, ipAddress, timestamp);
            }

            return Task.CompletedTask;
        }

        public Task LogSystemEventAsync(string eventType, string description, DateTime timestamp, int? userId = null)
        {
            _logger.LogInformation(
                "SYSTEM: Event={EventType}, Description={Description}, Timestamp={Timestamp}, UserId={UserId}",
                eventType, description, timestamp, userId);

            return Task.CompletedTask;
        }
    }
}