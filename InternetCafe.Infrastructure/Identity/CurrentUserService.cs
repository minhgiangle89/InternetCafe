using InternetCafe.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace InternetCafe.Infrastructure.Identity
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public int? UserId
        {
            get
            {
                var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
                return userId != null ? int.Parse(userId.Value) : null;
            }
        }
    }
}