using InternetCafe.Domain.Entities;
using InternetCafe.Domain.Enums;
using InternetCafe.Domain.Interfaces;
using InternetCafe.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InternetCafe.Infrastructure.Persistence
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeDatabaseAsync(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                var passwordHasher = services.GetRequiredService<IPasswordHasher>();

                // Apply migrations
                await context.Database.MigrateAsync();
                logger.LogInformation("Successfully applied database migrations");

                // Seed data
                await SeedDataAsync(context, passwordHasher, logger);
                logger.LogInformation("Successfully seeded the database");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database");
                throw;
            }
        }

        private static async Task SeedDataAsync(ApplicationDbContext context, IPasswordHasher passwordHasher, ILogger logger)
        {
            // Seed admin user if no users exist
            if (!await context.Users.AnyAsync())
            {
                logger.LogInformation("Seeding admin user...");

                var adminUser = new User
                {
                    Username = "admin",
                    Email = "admin@internetcafe.com",
                    PasswordHash = passwordHasher.HashPassword("Admin@123"),
                    FullName = "System Administrator",
                    PhoneNumber = "0123456789",
                    Address = "Internet Cafe Office",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    Role = (int)UserRole.Admin,
                    Status = (int)UserStatus.Active,
                    LastLoginTime = DateTime.Now,
                    Creation_Timestamp = DateTime.Now,
                    Creation_EmpId = 1,
                    LastUpdated_Timestamp = DateTime.Now,
                    LastUpdated_EmpId = 1
                };

                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();

                // Create account for admin
                var adminAccount = new Account
                {
                    UserId = adminUser.Id,
                    Balance = 0,
                    LastDepositDate = DateTime.Now,
                    LastUsageDate = DateTime.Now,
                    Creation_Timestamp = DateTime.Now,
                    Creation_EmpId = 1,
                    LastUpdated_Timestamp = DateTime.Now,
                    LastUpdated_EmpId = 1
                };

                await context.Accounts.AddAsync(adminAccount);
                await context.SaveChangesAsync();
            }

            // Seed default computers if no computers exist
            if (!await context.Computers.AnyAsync())
            {
                logger.LogInformation("Seeding default computers...");

                for (int i = 1; i <= 10; i++)
                {
                    var computer = new Computer
                    {
                        Name = $"PC-{i:D2}",
                        IPAddress = $"192.168.1.{i + 100}",
                        Specifications = "Intel Core i5, 16GB RAM, 512GB SSD, NVIDIA GTX 1660",
                        Location = $"Station {i}",
                        Status = (int)ComputerStatus.Available,
                        HourlyRate = 10000, // 10,000 VND per hour
                        LastMaintenanceDate = DateTime.Now,
                        Creation_Timestamp = DateTime.Now,
                        Creation_EmpId = 1,
                        LastUpdated_Timestamp = DateTime.Now,
                        LastUpdated_EmpId = 1
                    };

                    await context.Computers.AddAsync(computer);
                }

                await context.SaveChangesAsync();
            }
        }
    }
}