using InternetCafe.Domain.Entities;
using InternetCafe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternetCafe.Infrastructure.Persistence
{
    public class SessionConfiguration : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> entity)
        {
            entity.ToTable("Sessions");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();

            entity.Property(e => e.StartTime).IsRequired();
            entity.Property(e => e.EndTime);

            entity.Property(e => e.TotalCost).HasColumnType("decimal(18,2)").HasDefaultValue(0);

            entity.Property(e => e.Status).HasConversion<string>();

            entity.Property(e => e.Notes).HasMaxLength(500);

            // Relationships
            entity.HasMany(s => s.Transactions)
             .WithOne(t => t.Session)
             .HasForeignKey(t => t.SessionId);
        }
    }
}
