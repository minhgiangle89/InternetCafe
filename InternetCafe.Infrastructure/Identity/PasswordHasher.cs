using InternetCafe.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;

namespace InternetCafe.Infrastructure.Identity
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32; // 256 bit
        private const int Iterations = 10000;
        private readonly ILogger<PasswordHasher> _logger;

        public PasswordHasher(ILogger<PasswordHasher> logger)
        {
            _logger = logger;
        }

        public string HashPassword(string password)
        {
            try
            {
                using var algorithm = new Rfc2898DeriveBytes(
                    password,
                    SaltSize,
                    Iterations,
                    HashAlgorithmName.SHA256);

                var key = Convert.ToBase64String(algorithm.GetBytes(KeySize));
                var salt = Convert.ToBase64String(algorithm.Salt);

                return $"{Iterations}.{salt}.{key}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while hashing password");
                throw;
            }
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            try
            {
                // Check if the hash has the correct format
                var parts = passwordHash.Split('.');
                if (parts.Length != 3)
                {
                    _logger.LogWarning("Invalid password hash format");
                    return false;
                }

                var iterations = int.Parse(parts[0]);
                var salt = Convert.FromBase64String(parts[1]);
                var key = Convert.FromBase64String(parts[2]);

                // Verify the password
                using var algorithm = new Rfc2898DeriveBytes(
                    password,
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256);

                var keyToVerify = algorithm.GetBytes(KeySize);

                return CryptographicOperations.FixedTimeEquals(keyToVerify, key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while verifying password");
                return false;
            }
        }
    }
}