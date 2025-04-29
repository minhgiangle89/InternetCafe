using InternetCafe.Application.DTOs.Session;


namespace InternetCafe.Application.DTOs.Computer
{
    public class ComputerDetailsDTO : ComputerDTO
    {
        public SessionDTO? CurrentSession { get; set; }
        public ICollection<SessionSummaryDTO> RecentSessions { get; set; } = new List<SessionSummaryDTO>();
    }
}
