using InternetCafe.Application.DTOs.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetCafe.Application.DTOs.Account
{
    public class AccountDetailsDTO : AccountDTO
    {
        public ICollection<TransactionDTO> RecentTransactions { get; set; } = new List<TransactionDTO>();
    }
}
