using InternetCafe.Domain.Entities;
using InternetCafe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace InternetCafe.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {

        private readonly ICurrentUserService _currentUserService;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUserService) : base(options)
        {
            _currentUserService = currentUserService;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Computer> Computers { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Session> Sessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            HandleAuditInfo();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            HandleAuditInfo();
            return base.SaveChanges();
        }

        private void HandleAuditInfo()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            foreach (var entry in entries)
            {
                var now = TimeZoneInfo.ConvertTime(DateTime.Now, vietnamTimeZone);
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.Creation_Timestamp = now;
                        entry.Entity.Creation_EmpId = _currentUserService.UserId ?? 0;
                        entry.Entity.LastUpdated_Timestamp = now;
                        entry.Entity.LastUpdated_EmpId = _currentUserService.UserId ?? 0;
                        break;

                    case EntityState.Modified:
                        entry.Entity.LastUpdated_Timestamp = now;
                        entry.Entity.LastUpdated_EmpId = _currentUserService.UserId ?? 0;

                        entry.Property(x => x.Creation_Timestamp).IsModified = false;
                        entry.Property(x => x.Creation_EmpId).IsModified = false;
                        break;
                }
            }
        }
    }
}

