using InternetCafe.Domain.Entities;
using InternetCafe.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InternetCafe.Application.Interfaces.Repositories
{
    public interface IComputerRepository : IRepository<Computer>
    {
        Task<IReadOnlyList<Computer>> GetAvailableComputersAsync();
        Task<IReadOnlyList<Computer>> GetByStatusAsync(ComputerStatus status);
        Task UpdateStatusAsync(int computerId, ComputerStatus status);
    }
}