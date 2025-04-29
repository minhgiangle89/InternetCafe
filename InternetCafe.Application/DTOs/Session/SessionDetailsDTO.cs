using InternetCafe.Application.DTOs.Transaction;

namespace InternetCafe.Application.DTOs.Session
{
    public class SessionDetailsDTO : SessionDTO
    {
        public ICollection<TransactionDTO> Transactions { get; set; } = new List<TransactionDTO>();
    }
}
