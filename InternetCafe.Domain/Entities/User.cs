using InternetCafe.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace InternetCafe.Domain.Entities
{
    public class User : BaseEntity
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public UserRole Role { get; set; } = UserRole.Customer;
        public UserStatus Status { get; set; } = UserStatus.Active;
        public DateTime LastLoginTime { get; set; }

        // Navigation properties
        public Account? Account { get; set; }
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
