using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetCafe.Application.DTOs.Account
{
    public class DepositDTO
    {
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public int PaymentMethod { get; set; }
        public string? ReferenceNumber { get; set; }
    }
}
