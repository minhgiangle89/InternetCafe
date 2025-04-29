using InternetCafe.Application.DTOs.Account;
using InternetCafe.Application.DTOs.Transaction;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace InternetCafe.Application.Interfaces.Services
{
    public interface IAccountService
    {
        Task<AccountDTO> CreateAccountAsync(int userId);
        Task<decimal> GetBalanceAsync(int accountId);
        Task<decimal> GetBalanceByUserIdAsync(int userId);
        Task<TransactionDTO> DepositAsync(DepositDTO depositDTO);
        Task<TransactionDTO> WithdrawAsync(WithdrawDTO withdrawDTO);
        Task<TransactionDTO> ChargeForSessionAsync(int accountId, int sessionId, decimal amount);
        Task<bool> HasSufficientBalanceAsync(int accountId, decimal amount);
        Task<AccountDetailsDTO> GetAccountWithTransactionsAsync(int accountId);
        Task<AccountDTO> GetAccountByUserIdAsync(int userId);
        Task<IEnumerable<TransactionDTO>> GetTransactionsByAccountIdAsync(int accountId, int pageNumber, int pageSize);
    }
}