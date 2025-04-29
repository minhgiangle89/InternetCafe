using InternetCafe.Application.DTOs.Account;
using InternetCafe.Application.DTOs.Session;

namespace InternetCafe.Application.DTOs.User
{
    public class UserDetailsDto : UserDto
    {
        public AccountDTO? Account { get; set; }
        public ICollection<SessionSummaryDTO> RecentSessions { get; set; } = new List<SessionSummaryDTO>();
    }
}
