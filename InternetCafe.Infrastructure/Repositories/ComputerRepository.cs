using InternetCafe.Domain.Entities;
using InternetCafe.Domain.Enums;
using InternetCafe.Application.Interfaces.Repositories;
using InternetCafe.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
                .Where(c => c.ComputerStatus == (int)ComputerStatus.Available && c.Status == (int)Status.Active)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Computer>> GetByStatusAsync(ComputerStatus computerStatus)
        {
            return await _dbSet
                .Where(c => c.ComputerStatus == (int)computerStatus && c.Status == (int)Status.Active)
                .ToListAsync();
        }

        public async Task UpdateStatusAsync(int computerId, ComputerStatus computerStatus)
        {
            var computer = await _dbSet.FindAsync(computerId);
            if (computer != null && computer.Status == (int)Status.Active) 
            {
                computer.ComputerStatus = (int)computerStatus;
                if (computerStatus == ComputerStatus.InUse)
                {
                    computer.LastUsedDate = DateTime.Now;
                }
                else if (computerStatus == ComputerStatus.Maintenance)
                {
                    computer.LastMaintenanceDate = DateTime.Now;
                }
                _dbContext.Entry(computer).State = EntityState.Modified;
            }
        }
    }
}