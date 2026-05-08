using AirWatch.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirWatch.Infrastructure.Data.Configurations;

public class SensorCoefficientConfiguration : IEntityTypeConfiguration<SensorCoefficient>
{
    public void Configure(EntityTypeBuilder<SensorCoefficient> builder)
    {
        builder.ToTable("sensor_coefficients");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.SensorType).HasMaxLength(10).IsRequired();
        builder.Property(c => c.GasTarget).HasMaxLength(20).IsRequired();
        builder.Property(c => c.CoefA).IsRequired();
        builder.Property(c => c.CoefB).IsRequired();
        builder.Property(c => c.RatioMin).IsRequired();
        builder.Property(c => c.RatioMax).IsRequired();

        builder.HasIndex(c => new { c.SensorType, c.GasTarget }).IsUnique();
    }
}
