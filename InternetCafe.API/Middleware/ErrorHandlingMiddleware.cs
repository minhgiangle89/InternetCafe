using InternetCafe.Domain.Common;
using InternetCafe.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Net;
using System.Security.Authentication;
using System.Text.Json;
using System.Threading.Tasks;

namespace InternetCafe.API.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = exception switch
            {
                UserNotFoundException => (int)HttpStatusCode.NotFound,
                ComputerNotAvailableException => (int)HttpStatusCode.Conflict,
                SessionNotFoundException => (int)HttpStatusCode.NotFound,
                DuplicateUserException => (int)HttpStatusCode.Conflict,
                InsufficientBalanceException => (int)HttpStatusCode.PaymentRequired,
                AuthenticationException => (int)HttpStatusCode.Unauthorized,
                UnauthorizedAccessException => (int)HttpStatusCode.Forbidden,
                _ => (int)HttpStatusCode.InternalServerError
            };

            context.Response.StatusCode = statusCode;

            var response = Result.Failure(GetErrorMessage(exception));
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(json);
        }

        private static string GetErrorMessage(Exception exception)
        {
            // For domain exceptions, use the message directly
            if (exception is DomainException ||
                exception is AuthenticationException ||
                exception is UnauthorizedAccessException)
            {
                return exception.Message;
            }

            // For other exceptions, provide a generic message
            return "An error occurred while processing your request.";
        }
    }
}