using System.ComponentModel.DataAnnotations;

namespace InternetCafe.Application.DTOs.Authentication.Models
{
    public class RefreshTokenRequest
    {
        [Required]
        public string AccessToken { get; set; } = string.Empty;

        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}