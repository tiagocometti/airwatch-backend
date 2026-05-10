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

        builder.Property(d => d.ExternalId)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(d => d.ExternalId)
            .IsUnique();

        builder.Property(d => d.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(d => d.Location)
            .HasMaxLength(200);

        builder.Property(d => d.IsActive).IsRequired();
        builder.Property(d => d.IsOnline).IsRequired();
        builder.Property(d => d.LastSeen);
        builder.Property(d => d.RegisteredAt).IsRequired();
        builder.Property(d => d.UserId).IsRequired();

        builder.Property(d => d.RlMq3).IsRequired().HasDefaultValue(10000.0);
        builder.Property(d => d.RlMq5).IsRequired().HasDefaultValue(10000.0);
        builder.Property(d => d.RlMq135).IsRequired().HasDefaultValue(10000.0);

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
