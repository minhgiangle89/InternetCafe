using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetCafe.Application.DTOs.Account
{
    public class WithdrawDTO
    {
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public string? Reason { get; set; }
    }
}
