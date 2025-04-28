using InternetCafe.Domain.Entities;
using InternetCafe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace InternetCafe.Infrastructure.Persistence
{
    public class ComputerConfiguration : IEntityTypeConfiguration<Computer>
    {
        public void Configure(EntityTypeBuilder<Computer> entity)
        {
            entity.ToTable("Computers");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();

            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();

            entity.Property(e => e.IPAddress).IsRequired().HasMaxLength(15);
            entity.HasIndex(e => e.IPAddress).IsUnique();

            entity.Property(e => e.Specifications).IsRequired();
            entity.Property(e => e.Location).IsRequired().HasMaxLength(100);

            entity.Property(e => e.ComputerStatus).HasColumnType("int");

            entity.Property(e => e.HourlyRate).HasColumnType("decimal(18,2)");

            // Relationships
            entity.HasMany(c => c.Sessions)
                .WithOne(s => s.Computer)
                .HasForeignKey(s => s.ComputerId);

        }
    }
}
