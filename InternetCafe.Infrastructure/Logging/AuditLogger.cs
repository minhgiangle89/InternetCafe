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
                string.Format("AUDIT: Action={0}, Entity={1}, EntityId={2}, UserId={3}, Timestamp={4}, Details={5}",
                action, entityName, entityId, userId, timestamp, details));

            return Task.CompletedTask;
        }

        public Task LogLoginAttemptAsync(string username, bool success, string ipAddress, DateTime timestamp)
        {
            if (success)
            {
                _logger.LogInformation(
                    string.Format("LOGIN: User={0} successfully logged in from IP={1} at {2}",
                    username, ipAddress, timestamp));
            }
            else
            {
                _logger.LogWarning(
                    string.Format("LOGIN FAILED: User={0} failed to log in from IP={1} at {2}",
                    username, ipAddress, timestamp));
            }

            return Task.CompletedTask;
        }

        public Task LogSystemEventAsync(string eventType, string description, DateTime timestamp, int? userId = null)
        {
            _logger.LogInformation(
                string.Format("SYSTEM: Event={0}, Description={1}, Timestamp={2}, UserId={3}",
                eventType, description, timestamp, userId));

            return Task.CompletedTask;
        }
    }
}