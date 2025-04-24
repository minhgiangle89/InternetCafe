using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetCafe.Domain.Exceptions
{
    public class InsufficientBalanceException : DomainException
    {
        public InsufficientBalanceException() : base("Account has insufficient balance.")
        {
        }

        public InsufficientBalanceException(decimal currentBalance, decimal requiredAmount)
            : base($"Account has insufficient balance. Current balance: {currentBalance}, Required: {requiredAmount}")
        {
            CurrentBalance = currentBalance;
            RequiredAmount = requiredAmount;
        }

        public decimal CurrentBalance { get; }
        public decimal RequiredAmount { get; }
    }

}
