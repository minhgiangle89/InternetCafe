using InternetCafe.Application.DTOs.Transaction;

namespace InternetCafe.Application.DTOs.Session
{
    public class SessionDetailsDto : SessionDTO
    {
        public ICollection<TransactionDTO> Transactions { get; set; } = new List<TransactionDTO>();
    }
}
