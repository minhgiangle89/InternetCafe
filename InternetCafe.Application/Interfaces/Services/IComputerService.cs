using InternetCafe.Application.DTOs.Computer;
using InternetCafe.Domain.Enums;

namespace InternetCafe.Application.Interfaces.Services
{
    public interface IComputerService
    {
        Task<ComputerDTO> RegisterComputerAsync(CreateComputerDTO computerDTO);
        Task<IEnumerable<ComputerDTO>> GetAvailableComputersAsync();
        Task<ComputerDTO> GetComputerByIdAsync(int computerId);
        Task<ComputerDetailsDTO> GetComputerDetailsAsync(int computerId);
        Task SetComputerStatusAsync(ComputerStatusUpdateDTO updateDTO);
        Task<ComputerDTO> UpdateComputerAsync(int computerId, UpdateComputerDTO computerDTO);
        Task<bool> IsComputerAvailableAsync(int computerId);
        Task SetComputerMaintenanceAsync(int computerId, string reason);
        Task<IEnumerable<ComputerDTO>> GetComputersByStatusAsync(ComputerStatus status);
        Task<IEnumerable<ComputerDTO>> GetAllComputersAsync();
    }
}