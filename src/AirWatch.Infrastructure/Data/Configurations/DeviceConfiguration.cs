using AirWatch.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirWatch.Infrastructure.Data.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("devices");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");

        builder.Property(d => d.ExternalId)
            .HasMaxLength(100)
            .IsRequired()
            .HasColumnName("external_id");

        builder.HasIndex(d => d.ExternalId)
            .IsUnique();

        builder.Property(d => d.Name)
            .HasMaxLength(150)
            .IsRequired()
            .HasColumnName("name");

        builder.Property(d => d.IsActive).IsRequired().HasColumnName("is_active");
        builder.Property(d => d.IsOnline).IsRequired().HasColumnName("is_online");
        builder.Property(d => d.LastSeen).HasColumnName("last_seen");
        builder.Property(d => d.RegisteredAt).IsRequired().HasColumnName("registered_at");
        builder.Property(d => d.UserId).IsRequired().HasColumnName("user_id");

        builder.Property(d => d.RlMq3).IsRequired().HasDefaultValue(10000.0).HasColumnName("rl_mq3");
        builder.Property(d => d.RlMq5).IsRequired().HasDefaultValue(10000.0).HasColumnName("rl_mq5");
        builder.Property(d => d.RlMq135).IsRequired().HasDefaultValue(10000.0).HasColumnName("rl_mq135");

        builder.HasIndex(d => d.UserId);

        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Measurements)
            .WithOne(m => m.Device)
            .HasForeignKey(m => m.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Calibrations)
            .WithOne(c => c.Device)
            .HasForeignKey(c => c.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
