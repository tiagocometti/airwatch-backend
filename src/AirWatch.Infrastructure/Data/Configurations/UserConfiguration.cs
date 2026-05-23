using AirWatch.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirWatch.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");

        builder.Property(u => u.Name)
            .HasMaxLength(100)
            .IsRequired()
            .HasColumnName("name");

        builder.Property(u => u.Email)
            .HasMaxLength(200)
            .IsRequired()
            .HasColumnName("email");

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(512)
            .IsRequired()
            .HasColumnName("password_hash");

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(u => u.EmailNotificationsEnabled)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("email_notifications_enabled");
    }
}
