using InternetCafe.Domain.Interfaces;
using InternetCafe.Domain.Interfaces.Repositories;
using InternetCafe.Infrastructure.Identity;
using InternetCafe.Infrastructure.Persistence;
using InternetCafe.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace InternetCafe.Infrastructure.Extensions
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // Database
            services.AddDatabaseServices(configuration);

            // Repositories
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IComputerRepository, ComputerRepository>();
            services.AddScoped<ISessionRepository, SessionRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();

            // UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Identity and Authentication
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IJwtService, JwtService>();

            return services;
        }
    }
}