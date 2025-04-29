using AutoMapper;
using InternetCafe.Application.DTOs.Account;
using InternetCafe.Application.DTOs.Transaction;
using InternetCafe.Application.Interfaces;
using InternetCafe.Application.Interfaces.Services;
using InternetCafe.Domain.Entities;
using InternetCafe.Domain.Enums;
using InternetCafe.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InternetCafe.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditLogger _auditLogger;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IAuditLogger auditLogger,
            ILogger<AccountService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AccountDTO> CreateAccountAsync(int userId)
        {
            try
            {
                // Check if user exists
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new UserNotFoundException(userId);
                }

                // Check if account already exists
                var existingAccount = await _unitOfWork.Accounts.GetByUserIdAsync(userId);
                if (existingAccount != null)
                {
                    _logger.LogWarning("Account already exists for user {UserId}", userId);
                    return _mapper.Map<AccountDTO>(existingAccount);
                }

                // Create new account
                var account = new Account
                {
                    UserId = userId,
                    Balance = 0,
                    LastDepositDate = DateTime.UtcNow,
                    LastUsageDate = DateTime.UtcNow
                };

                await _unitOfWork.Accounts.AddAsync(account);
                await _unitOfWork.CompleteAsync();

                // Log account creation
                await _auditLogger.LogActivityAsync(
                    "AccountCreated",
                    "Account",
                    account.Id,
                    _currentUserService.UserId ?? 0,
                    DateTime.UtcNow,
                    $"Account created for user {userId}");

                var accountDto = _mapper.Map<AccountDTO>(account);
                accountDto.UserName = user.Username;
                return accountDto;
            }
            catch (Exception ex) when (ex is not UserNotFoundException)
            {
                _logger.LogError(ex, "Error creating account for user {UserId}", userId);
                throw;
            }
        }

        public async Task<decimal> GetBalanceAsync(int accountId)
        {
            var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);
            if (account == null)
            {
                throw new Exception($"Account with ID {accountId} not found.");
            }

            return account.Balance;
        }

        public async Task<decimal> GetBalanceByUserIdAsync(int userId)
        {
            var account = await _unitOfWork.Accounts.GetByUserIdAsync(userId);
            if (account == null)
            {
                throw new Exception($"Account for user with ID {userId} not found.");
            }

            return account.Balance;
        }

        public async Task<TransactionDTO> DepositAsync(DepositDTO depositDTO)
        {
            try
            {
                if (depositDTO.Amount <= 0)
                {
                    throw new ArgumentException("Deposit amount must be greater than zero.");
                }

                await _unitOfWork.BeginTransactionAsync();

                // Get account
                var account = await _unitOfWork.Accounts.GetByIdAsync(depositDTO.AccountId);
                if (account == null)
                {
                    throw new Exception($"Account with ID {depositDTO.AccountId} not found.");
                }

                // Update account balance
                account.Balance += depositDTO.Amount;
                account.LastDepositDate = DateTime.UtcNow;
                await _unitOfWork.Accounts.UpdateAsync(account);

                // Create transaction record
                var transaction = new Transaction
                {
                    AccountId = depositDTO.AccountId,
                    Amount = depositDTO.Amount,
                    Type = TransactionType.Deposit,
                    PaymentMethod = (PaymentMethod)depositDTO.PaymentMethod,
                    ReferenceNumber = depositDTO.ReferenceNumber,
                    Description = "Deposit to account",
                    UserId = _currentUserService.UserId
                };

                await _unitOfWork.Transactions.AddAsync(transaction);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Log deposit
                await _auditLogger.LogActivityAsync(
                    "AccountDeposit",
                    "Account",
                    account.Id,
                    _currentUserService.UserId ?? 0,
                    DateTime.UtcNow,
                    $"Deposit of {depositDTO.Amount} to account {account.Id}");

                var transactionDto = _mapper.Map<TransactionDTO>(transaction);
                if (transaction.UserId.HasValue)
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(transaction.UserId.Value);
                    transactionDto.UserName = user?.Username;
                }

                return transactionDto;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error depositing to account {AccountId}", depositDTO.AccountId);
                throw;
            }
        }

        public async Task<TransactionDTO> WithdrawAsync(WithdrawDTO withdrawDTO)
        {
            try
            {
                if (withdrawDTO.Amount <= 0)
                {
                    throw new ArgumentException("Withdrawal amount must be greater than zero.");
                }

                await _unitOfWork.BeginTransactionAsync();

                // Get account
                var account = await _unitOfWork.Accounts.GetByIdAsync(withdrawDTO.AccountId);
                if (account == null)
                {
                    throw new Exception($"Account with ID {withdrawDTO.AccountId} not found.");
                }

                // Check if balance is sufficient
                if (account.Balance < withdrawDTO.Amount)
                {
                    throw new InsufficientBalanceException(account.Balance, withdrawDTO.Amount);
                }

                // Update account balance
                account.Balance -= withdrawDTO.Amount;
                account.LastUsageDate = DateTime.UtcNow;
                await _unitOfWork.Accounts.UpdateAsync(account);

                // Create transaction record
                var transaction = new Transaction
                {
                    AccountId = withdrawDTO.AccountId,
                    Amount = -withdrawDTO.Amount, // Negative amount for withdrawal
                    Type = TransactionType.Withdrawal,
                    Description = withdrawDTO.Reason ?? "Withdrawal from account",
                    UserId = _currentUserService.UserId
                };

                await _unitOfWork.Transactions.AddAsync(transaction);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Log withdrawal
                await _auditLogger.LogActivityAsync(
                    "AccountWithdrawal",
                    "Account",
                    account.Id,
                    _currentUserService.UserId ?? 0,
                    DateTime.UtcNow,
                    $"Withdrawal of {withdrawDTO.Amount} from account {account.Id}");

                var transactionDto = _mapper.Map<TransactionDTO>(transaction);
                if (transaction.UserId.HasValue)
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(transaction.UserId.Value);
                    transactionDto.UserName = user?.Username;
                }

                return transactionDto;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error withdrawing from account {AccountId}", withdrawDTO.AccountId);
                throw;
            }
        }

        public async Task<TransactionDTO> ChargeForSessionAsync(int accountId, int sessionId, decimal amount)
        {
            try
            {
                if (amount <= 0)
                {
                    throw new ArgumentException("Charge amount must be greater than zero.");
                }

                await _unitOfWork.BeginTransactionAsync();

                // Get account
                var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);
                if (account == null)
                {
                    throw new Exception($"Account with ID {accountId} not found.");
                }

                // Check if balance is sufficient
                if (account.Balance < amount)
                {
                    throw new InsufficientBalanceException(account.Balance, amount);
                }

                // Get session
                var session = await _unitOfWork.Sessions.GetByIdAsync(sessionId);
                if (session == null)
                {
                    throw new SessionNotFoundException(sessionId);
                }

                // Update account balance
                account.Balance -= amount;
                account.LastUsageDate = DateTime.UtcNow;
                await _unitOfWork.Accounts.UpdateAsync(account);

                // Create transaction record
                var transaction = new Transaction
                {
                    AccountId = accountId,
                    Amount = -amount, // Negative amount for charge
                    Type = TransactionType.ComputerUsage,
                    Description = $"Charge for session #{sessionId}",
                    UserId = session.UserId,
                    SessionId = sessionId
                };

                await _unitOfWork.Transactions.AddAsync(transaction);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Log session charge
                await _auditLogger.LogActivityAsync(
                    "SessionCharge",
                    "Account",
                    account.Id,
                    session.UserId,
                    DateTime.UtcNow,
                    $"Charge of {amount} for session {sessionId}");

                var transactionDto = _mapper.Map<TransactionDTO>(transaction);
                var user = await _unitOfWork.Users.GetByIdAsync(session.UserId);
                transactionDto.UserName = user?.Username;

                return transactionDto;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error charging account {AccountId} for session {SessionId}", accountId, sessionId);
                throw;
            }
        }

        public async Task<bool> HasSufficientBalanceAsync(int accountId, decimal amount)
        {
            var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);
            if (account == null)
            {
                throw new Exception($"Account with ID {accountId} not found.");
            }

            return account.Balance >= amount;
        }

        public async Task<AccountDetailsDTO> GetAccountWithTransactionsAsync(int accountId)
        {
            var account = await _unitOfWork.Accounts.GetWithTransactionsAsync(accountId);
            if (account == null)
            {
                throw new Exception($"Account with ID {accountId} not found.");
            }

            var result = _mapper.Map<AccountDetailsDTO>(account);

            // Load username
            var user = await _unitOfWork.Users.GetByIdAsync(account.UserId);
            result.UserName = user?.Username ?? "Unknown";

            // Get recent transactions (last 10)
            var transactions = await _unitOfWork.Transactions.GetByAccountIdAsync(accountId);
            result.RecentTransactions = _mapper.Map<ICollection<TransactionDTO>>(
                transactions.Take(10).ToList());

            // Load usernames for transaction records
            foreach (var transaction in result.RecentTransactions)
            {
                if (transaction.UserId.HasValue)
                {
                    var transactionUser = await _unitOfWork.Users.GetByIdAsync(transaction.UserId.Value);
                    transaction.UserName = transactionUser?.Username;
                }
            }

            return result;
        }

        public async Task<AccountDTO> GetAccountByUserIdAsync(int userId)
        {
            var account = await _unitOfWork.Accounts.GetByUserIdAsync(userId);
            if (account == null)
            {
                throw new Exception($"Account for user with ID {userId} not found.");
            }

            var result = _mapper.Map<AccountDTO>(account);

            // Load username
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            result.UserName = user?.Username ?? "Unknown";

            return result;
        }

        public async Task<IEnumerable<TransactionDTO>> GetTransactionsByAccountIdAsync(int accountId, int pageNumber, int pageSize)
        {
            // Validate account exists
            var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);
            if (account == null)
            {
                throw new Exception($"Account with ID {accountId} not found.");
            }

            // Get transactions
            var transactions = await _unitOfWork.Transactions.GetByAccountIdAsync(accountId);

            // Apply pagination
            var paginatedTransactions = transactions
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = _mapper.Map<IEnumerable<TransactionDTO>>(paginatedTransactions);

            // Load usernames
            foreach (var transaction in result)
            {
                if (transaction.UserId.HasValue)
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(transaction.UserId.Value);
                    transaction.UserName = user?.Username;
                }
            }

            return result;
        }
    }
}