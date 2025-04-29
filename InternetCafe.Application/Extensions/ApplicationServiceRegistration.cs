using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using MediatR;
using FluentValidation;
using System;
using InternetCafe.Application.Interfaces.Services;
using InternetCafe.Application.Services;

namespace InternetCafe.Application.Extensions
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Register AutoMapper
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // Register application services
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<IComputerService, ComputerService>();
            services.AddScoped<IStatisticsService, StatisticsService>();

            return services;
        }
    }
}