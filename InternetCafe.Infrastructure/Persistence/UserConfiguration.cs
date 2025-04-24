using InternetCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternetCafe.Infrastructure.Persistence
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> modelBuilder)
        {

            modelBuilder.ToTable("Users");

            modelBuilder.HasKey(e => e.Id);
            modelBuilder.Property(e => e.Id).UseIdentityColumn();

            modelBuilder.Property(e => e.Username).IsRequired().HasMaxLength(50);
            modelBuilder.HasIndex(e => e.Username).IsUnique();

            modelBuilder.Property(e => e.Email).IsRequired().HasMaxLength(100);
            modelBuilder.HasIndex(e => e.Email).IsUnique();

            modelBuilder.Property(e => e.PasswordHash).IsRequired();
            modelBuilder.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            modelBuilder.Property(e => e.PhoneNumber).HasMaxLength(20);
            modelBuilder.Property(e => e.Address).HasMaxLength(200);

            modelBuilder.Property(e => e.Role).HasConversion<string>();
            modelBuilder.Property(e => e.Status).HasConversion<string>();

            // Relationships
            modelBuilder.HasOne(u => u.Account)
                    .WithOne(a => a.User)
                    .HasForeignKey<Account>(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
