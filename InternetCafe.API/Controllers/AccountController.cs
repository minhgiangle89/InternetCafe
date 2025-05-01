using InternetCafe.API.Common;
using InternetCafe.Application.DTOs.Account;
using InternetCafe.Application.DTOs.Transaction;
using InternetCafe.Application.Interfaces;
using InternetCafe.Application.Interfaces.Services;
using InternetCafe.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
        [ProducesResponseType(typeof(ApiResponse<AccountDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<AccountDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<AccountDTO>), 403)]
        [ProducesResponseType(typeof(ApiResponse<AccountDTO>), 404)]
        [ProducesResponseType(typeof(ApiResponse<AccountDTO>), 500)]
        public async Task<ActionResult<ApiResponse<AccountDTO>>> GetAccountByUserId(int userId)
        {
            try
            {
                var currentUserId = _currentUserService.UserId;
                if (currentUserId != userId && !User.IsInRole("2"))
                {
                    return Forbid();
                }

                var account = await _accountService.GetAccountByUserIdAsync(userId);
                return Ok(ApiResponseFactory.Success(account, "Thông tin tài khoản được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin tài khoản cho người dùng có ID {UserId}", userId);

                if (ex.Message.Contains("not found"))
                    return NotFound(ApiResponseFactory.Fail<AccountDTO>($"Không tìm thấy tài khoản cho người dùng có ID {userId}"));

                return StatusCode(500, ApiResponseFactory.Fail<AccountDTO>("Lỗi server khi lấy thông tin tài khoản"));
            }
        }

        [HttpGet("{accountId}")]
        [ProducesResponseType(typeof(ApiResponse<AccountDetailsDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<AccountDetailsDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<AccountDetailsDTO>), 403)]
        [ProducesResponseType(typeof(ApiResponse<AccountDetailsDTO>), 404)]
        [ProducesResponseType(typeof(ApiResponse<AccountDetailsDTO>), 500)]
        public async Task<ActionResult<ApiResponse<AccountDetailsDTO>>> GetAccountDetails(int accountId)
        {
            try
            {
                var account = await _accountService.GetAccountWithTransactionsAsync(accountId);

                var currentUserId = _currentUserService.UserId;
                if (account.UserId != currentUserId && !User.IsInRole("2"))
                {
                    return Forbid();
                }

                return Ok(ApiResponseFactory.Success(account, "Chi tiết tài khoản được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết tài khoản có ID {AccountId}", accountId);

                if (ex.Message.Contains("not found"))
                    return NotFound(ApiResponseFactory.Fail<AccountDetailsDTO>($"Không tìm thấy tài khoản có ID {accountId}"));

                return StatusCode(500, ApiResponseFactory.Fail<AccountDetailsDTO>("Lỗi server khi lấy chi tiết tài khoản"));
            }
        }

        [HttpGet("{accountId}/balance")]
        [ProducesResponseType(typeof(ApiResponse<decimal>), 200)]
        [ProducesResponseType(typeof(ApiResponse<decimal>), 401)]
        [ProducesResponseType(typeof(ApiResponse<decimal>), 403)]
        [ProducesResponseType(typeof(ApiResponse<decimal>), 404)]
        [ProducesResponseType(typeof(ApiResponse<decimal>), 500)]
        public async Task<ActionResult<ApiResponse<decimal>>> GetBalance(int accountId)
        {
            try
            {
                var account = await _accountService.GetAccountWithTransactionsAsync(accountId);

                var currentUserId = _currentUserService.UserId;
                if (account.UserId != currentUserId && !User.IsInRole("2"))
                {
                    return Forbid();
                }

                var balance = await _accountService.GetBalanceAsync(accountId);
                return Ok(ApiResponseFactory.Success(balance, "Số dư tài khoản được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy số dư tài khoản có ID {AccountId}", accountId);

                if (ex.Message.Contains("not found"))
                    return NotFound(ApiResponseFactory.Fail<decimal>($"Không tìm thấy tài khoản có ID {accountId}"));

                return StatusCode(500, ApiResponseFactory.Fail<decimal>("Lỗi server khi lấy số dư tài khoản"));
            }
        }

        [HttpPost("deposit")]
        [ProducesResponseType(typeof(ApiResponse<TransactionDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<TransactionDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<TransactionDTO>), 403)]
        [ProducesResponseType(typeof(ApiResponse<TransactionDTO>), 400)]
        [ProducesResponseType(typeof(ApiResponse<TransactionDTO>), 404)]
        [ProducesResponseType(typeof(ApiResponse<TransactionDTO>), 500)]
        public async Task<ActionResult<ApiResponse<TransactionDTO>>> Deposit([FromBody] DepositDTO depositDTO)
        {
            try
            {
                if (depositDTO.Amount <= 0)
                {
                    return BadRequest(ApiResponseFactory.Fail<TransactionDTO>("Số tiền nạp phải lớn hơn 0"));
                }
                var account = await _accountService.GetAccountWithTransactionsAsync(depositDTO.AccountId);
                var currentUserId = _currentUserService.UserId;
                if (account.UserId != currentUserId && !User.IsInRole("1") && !User.IsInRole("2"))
                {
                    return Forbid();
                }

                var transaction = await _accountService.DepositAsync(depositDTO);
                return Ok(ApiResponseFactory.Success(transaction, "Nạp tiền thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi nạp tiền vào tài khoản có ID {AccountId}", depositDTO.AccountId);

                if (ex.Message.Contains("not found"))
                    return NotFound(ApiResponseFactory.Fail<TransactionDTO>($"Không tìm thấy tài khoản có ID {depositDTO.AccountId}"));

                return BadRequest(ApiResponseFactory.Fail<TransactionDTO>("Nạp tiền thất bại: " + ex.Message));
            }
        }

        [HttpPost("withdraw")]
        [Authorize(Roles = "1,2")]
        [ProducesResponseType(typeof(ApiResponse<TransactionDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<TransactionDTO>), 401)]
        [ProducesResponseType(typeof(ApiResponse<TransactionDTO>), 403)]
        [ProducesResponseType(typeof(ApiResponse<TransactionDTO>), 400)]
        [ProducesResponseType(typeof(ApiResponse<TransactionDTO>), 404)]
        [ProducesResponseType(typeof(ApiResponse<TransactionDTO>), 500)]
        public async Task<ActionResult<ApiResponse<TransactionDTO>>> Withdraw([FromBody] WithdrawDTO withdrawDTO)
        {
            try
            {
                if (withdrawDTO.Amount <= 0)
                {
                    return BadRequest(ApiResponseFactory.Fail<TransactionDTO>("Số tiền rút phải lớn hơn 0"));
                }

                var transaction = await _accountService.WithdrawAsync(withdrawDTO);
                return Ok(ApiResponseFactory.Success(transaction, "Rút tiền thành công"));
            }
            catch (InsufficientBalanceException ex)
            {
                _logger.LogWarning(ex, "Số dư không đủ để rút tiền từ tài khoản có ID {AccountId}", withdrawDTO.AccountId);
                return BadRequest(ApiResponseFactory.Fail<TransactionDTO>("Số dư không đủ để rút tiền: " + ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi rút tiền từ tài khoản có ID {AccountId}", withdrawDTO.AccountId);

                if (ex.Message.Contains("not found"))
                    return NotFound(ApiResponseFactory.Fail<TransactionDTO>($"Không tìm thấy tài khoản có ID {withdrawDTO.AccountId}"));

                return BadRequest(ApiResponseFactory.Fail<TransactionDTO>("Rút tiền thất bại: " + ex.Message));
            }
        }

        [HttpGet("{accountId}/transactions")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TransactionDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TransactionDTO>>), 401)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TransactionDTO>>), 403)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TransactionDTO>>), 404)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TransactionDTO>>), 500)]
        public async Task<ActionResult<ApiResponse<IEnumerable<TransactionDTO>>>> GetTransactions(
            int accountId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var account = await _accountService.GetAccountWithTransactionsAsync(accountId);

                var currentUserId = _currentUserService.UserId;
                if (account.UserId != currentUserId && !User.IsInRole("2"))
                {
                    return Forbid();
                }

                var transactions = await _accountService.GetTransactionsByAccountIdAsync(accountId, pageNumber, pageSize);
                return Ok(ApiResponseFactory.Success(transactions, "Danh sách giao dịch được tải thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách giao dịch cho tài khoản có ID {AccountId}", accountId);

                if (ex.Message.Contains("not found"))
                    return NotFound(ApiResponseFactory.Fail<IEnumerable<TransactionDTO>>($"Không tìm thấy tài khoản có ID {accountId}"));

                return StatusCode(500, ApiResponseFactory.Fail<IEnumerable<TransactionDTO>>("Lỗi server khi lấy danh sách giao dịch"));
            }
        }
    }
}