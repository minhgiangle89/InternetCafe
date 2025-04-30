using InternetCafe.Application.DTOs.Account;
using InternetCafe.Application.DTOs.Transaction;
using InternetCafe.Application.Interfaces;
using InternetCafe.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InternetCafe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IAccountService accountService,
            ICurrentUserService currentUserService,
            ILogger<AccountController> logger)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(AccountDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AccountDTO>> GetAccountByUserId(int userId)
        {
            try
            {
                // Ensure user can only access their own account unless they're admin
                var currentUserId = _currentUserService.UserId;
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (currentUserId != userId && userRole != "2") // Not own account and not admin
                {
                    return Forbid();
                }

                var account = await _accountService.GetAccountByUserIdAsync(userId);
                return Ok(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error retrieving account for user with ID {0}", userId));
                return ex.Message.Contains("not found") ? NotFound(new { Message = ex.Message }) :
                    StatusCode(500, new { Message = "An error occurred while retrieving account" });
            }
        }

        [HttpGet("{accountId}")]
        [ProducesResponseType(typeof(AccountDetailsDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AccountDetailsDTO>> GetAccountDetails(int accountId)
        {
            try
            {
                var account = await _accountService.GetAccountWithTransactionsAsync(accountId);

                // Ensure user can only access their own account unless they're admin
                var currentUserId = _currentUserService.UserId;
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (account.UserId != currentUserId && userRole != "2") // Not own account and not admin
                {
                    return Forbid();
                }

                return Ok(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error retrieving account details for account with ID {0}", accountId));
                return ex.Message.Contains("not found") ? NotFound(new { Message = ex.Message }) :
                    StatusCode(500, new { Message = "An error occurred while retrieving account details" });
            }
        }

        [HttpGet("{accountId}/balance")]
        [ProducesResponseType(typeof(decimal), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<decimal>> GetBalance(int accountId)
        {
            try
            {
                // Get the account to check the user ID
                var account = await _accountService.GetAccountWithTransactionsAsync(accountId);

                // Ensure user can only access their own account unless they're admin
                var currentUserId = _currentUserService.UserId;
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (account.UserId != currentUserId && userRole != "2") // Not own account and not admin
                {
                    return Forbid();
                }

                var balance = await _accountService.GetBalanceAsync(accountId);
                return Ok(balance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error retrieving balance for account with ID {0}", accountId));
                return ex.Message.Contains("not found") ? NotFound(new { Message = ex.Message }) :
                    StatusCode(500, new { Message = "An error occurred while retrieving balance" });
            }
        }

        [HttpPost("deposit")]
        [ProducesResponseType(typeof(TransactionDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<TransactionDTO>> Deposit([FromBody] DepositDTO depositDTO)
        {
            try
            {
                // Validate deposit amount
                if (depositDTO.Amount <= 0)
                {
                    return BadRequest(new { Message = "Deposit amount must be greater than zero" });
                }

                // Get the account to check the user ID
                var account = await _accountService.GetAccountWithTransactionsAsync(depositDTO.AccountId);

                // Ensure user can only deposit to their own account unless they're staff or admin
                var currentUserId = _currentUserService.UserId;
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (account.UserId != currentUserId && userRole != "1" && userRole != "2") // Not own account and not staff/admin
                {
                    return Forbid();
                }

                var transaction = await _accountService.DepositAsync(depositDTO);
                return Ok(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error depositing to account with ID {0}", depositDTO.AccountId));
                return ex.Message.Contains("not found") ? NotFound(new { Message = ex.Message }) :
                    BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("withdraw")]
        [Authorize(Roles = "1,2")] // Staff and Admin only
        [ProducesResponseType(typeof(TransactionDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<TransactionDTO>> Withdraw([FromBody] WithdrawDTO withdrawDTO)
        {
            try
            {
                // Validate withdrawal amount
                if (withdrawDTO.Amount <= 0)
                {
                    return BadRequest(new { Message = "Withdrawal amount must be greater than zero" });
                }

                var transaction = await _accountService.WithdrawAsync(withdrawDTO);
                return Ok(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error withdrawing from account with ID {0}", withdrawDTO.AccountId));

                if (ex.Message.Contains("not found"))
                    return NotFound(new { Message = ex.Message });
                else if (ex.Message.Contains("insufficient"))
                    return BadRequest(new { Message = ex.Message });
                else
                    return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("{accountId}/transactions")]
        [ProducesResponseType(typeof(IEnumerable<TransactionDTO>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<TransactionDTO>>> GetTransactions(
            int accountId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                // Get the account to check the user ID
                var account = await _accountService.GetAccountWithTransactionsAsync(accountId);

                // Ensure user can only access their own account unless they're admin
                var currentUserId = _currentUserService.UserId;
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (account.UserId != currentUserId && userRole != "2") // Not own account and not admin
                {
                    return Forbid();
                }

                var transactions = await _accountService.GetTransactionsByAccountIdAsync(accountId, pageNumber, pageSize);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error retrieving transactions for account with ID {0}", accountId));
                return ex.Message.Contains("not found") ? NotFound(new { Message = ex.Message }) :
                    StatusCode(500, new { Message = "An error occurred while retrieving transactions" });
            }
        }
    }
}