using InternetCafe.Domain.Entities;
using InternetCafe.Domain.Enums;
using System.Threading.Tasks;

namespace InternetCafe.Domain.Interfaces.Services
{
    public interface IUserService
    {
        Task<User> AuthenticateUserAsync(string username, string password);
        Task<User> RegisterUserAsync(User user, string password);
        Task UpdateUserAsync(User user);
        Task<bool> CheckPasswordAsync(User user, string password);
        Task ChangePasswordAsync(User user, string currentPassword, string newPassword);
        Task ChangeUserStatusAsync(int userId, UserStatus status);
        Task<User> GetUserByIdAsync(int userId);
        Task<User> GetUserByUsernameAsync(string username);
    }
}