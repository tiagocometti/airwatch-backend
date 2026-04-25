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

        builder.Property(d => d.IsActive)
            .IsRequired();

        builder.Property(d => d.RegisteredAt)
            .IsRequired();

        builder.HasMany(d => d.Measurements)
            .WithOne(m => m.Device)
            .HasForeignKey(m => m.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
