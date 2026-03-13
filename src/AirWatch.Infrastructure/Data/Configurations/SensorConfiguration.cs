using AirWatch.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirWatch.Infrastructure.Data.Configurations;

public class SensorConfiguration : IEntityTypeConfiguration<Sensor>
{
    public void Configure(EntityTypeBuilder<Sensor> builder)
    {
        builder.ToTable("sensors");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ExternalId)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(s => s.ExternalId)
            .IsUnique();

        builder.Property(s => s.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(s => s.Location)
            .HasMaxLength(200);

        builder.Property(s => s.IsActive)
            .IsRequired();

        builder.Property(s => s.RegisteredAt)
            .IsRequired();

        builder.HasMany(s => s.Measurements)
            .WithOne(m => m.Sensor)
            .HasForeignKey(m => m.SensorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
