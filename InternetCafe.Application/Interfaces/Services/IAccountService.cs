using InternetCafe.Domain.Entities;
using InternetCafe.Domain.Enums;
using System.Threading.Tasks;

namespace InternetCafe.Application.Interfaces.Services
{
    public interface IAccountService
    {
        Task<Account> CreateAccountAsync(int userId);
        Task<decimal> GetBalanceAsync(int accountId);
        Task<decimal> GetBalanceByUserIdAsync(int userId);
        Task<Transaction> DepositAsync(int accountId, decimal amount, PaymentMethod paymentMethod, string? referenceNumber = null);
        Task<Transaction> WithdrawAsync(int accountId, decimal amount, string? reason = null);
        Task<Transaction> ChargeForSessionAsync(int accountId, int sessionId, decimal amount);
        Task<bool> HasSufficientBalanceAsync(int accountId, decimal amount);
        Task<Account> GetAccountWithTransactionsAsync(int accountId);
        Task<Account> GetAccountByUserIdAsync(int userId);
    }
}