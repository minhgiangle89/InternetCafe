using InternetCafe.Application.Interfaces.Repositories;
using System;
using System.Threading.Tasks;

namespace InternetCafe.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IAccountRepository Accounts { get; }
        IComputerRepository Computers { get; }
        ISessionRepository Sessions { get; }
        ITransactionRepository Transactions { get; }

        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}