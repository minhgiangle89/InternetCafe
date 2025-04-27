using InternetCafe.Domain.Entities;
using InternetCafe.Domain.Enums;
using InternetCafe.Domain.Interfaces.Repositories;
using InternetCafe.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InternetCafe.Infrastructure.Repositories
{
    public class ComputerRepository : GenericRepository<Computer>, IComputerRepository
    {
        public ComputerRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<Computer>> GetAvailableComputersAsync()
        {
            return await _dbSet
                .Where(c => c.Status == ComputerStatus.Available)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Computer>> GetByStatusAsync(ComputerStatus status)
        {
            return await _dbSet
                .Where(c => c.Status == status)
                .ToListAsync();
        }

        public async Task UpdateStatusAsync(int computerId, ComputerStatus status)
        {
            var computer = await _dbSet.FindAsync(computerId);
            if (computer != null)
            {
                computer.Status = status;
                if (status == ComputerStatus.InUse)
                {
                    computer.LastUsedDate = DateTime.Now;
                }
                else if (status == ComputerStatus.Maintenance)
                {
                    computer.LastMaintenanceDate = DateTime.Now;
                }

                _dbContext.Entry(computer).State = EntityState.Modified;
            }
        }
    }
}