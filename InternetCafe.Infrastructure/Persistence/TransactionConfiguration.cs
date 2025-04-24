using InternetCafe.Domain.Entities;
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
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> entity)
        {
            entity.ToTable("Transactions");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();

            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();

            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.PaymentMethod).HasConversion<string>();

            entity.Property(e => e.ReferenceNumber).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);

            // Relationships
            entity.HasOne(t => t.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.Session)
                .WithMany(s => s.Transactions)
                .HasForeignKey(t => t.SessionId)
                .OnDelete(DeleteBehavior.SetNull);

        }
    }
}