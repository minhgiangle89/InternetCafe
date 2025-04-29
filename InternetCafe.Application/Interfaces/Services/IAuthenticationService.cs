using InternetCafe.Application.DTOs.Authentication.Models;
using System.Threading.Tasks;

namespace InternetCafe.Application.Interfaces.Services
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request);
        Task<AuthenticationResponse> RefreshTokenAsync(RefreshTokenRequest request);
    }
}