using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetCafe.Application.DTOs.User
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Role { get; set; }
        public DateTime LastLoginTime { get; set; }
        public DateTime CreationDate { get; set; }
        public int Status { get; set; }
    }

}
