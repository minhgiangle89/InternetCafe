using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetCafe.Application.DTOs.Transaction
{
    public class TransactionDTO
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public int? SessionId { get; set; }
        public decimal Amount { get; set; }
        public int Type { get; set; }
        public int? PaymentMethod { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Description { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
