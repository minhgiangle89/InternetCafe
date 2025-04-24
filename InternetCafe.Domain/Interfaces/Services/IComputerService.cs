using InternetCafe.Domain.Entities;
using InternetCafe.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InternetCafe.Domain.Interfaces.Services
{
    public interface IComputerService
    {
        Task<Computer> RegisterComputerAsync(Computer computer);
        Task<List<Computer>> GetAvailableComputersAsync();
        Task<Computer> GetComputerByIdAsync(int computerId);
        Task SetComputerStatusAsync(int computerId, ComputerStatus status);
        Task UpdateComputerAsync(Computer computer);
        Task<bool> IsComputerAvailableAsync(int computerId);
        Task SetComputerMaintenanceAsync(int computerId, string reason);
        Task<List<Computer>> GetComputersByStatusAsync(ComputerStatus status);
    }
}