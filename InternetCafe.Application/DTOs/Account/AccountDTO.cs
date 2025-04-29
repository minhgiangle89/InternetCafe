using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetCafe.Application.DTOs.Account
{
    public class AccountDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Balance { get; set; }
        public DateTime LastDepositDate { get; set; }
        public DateTime LastUsageDate { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
