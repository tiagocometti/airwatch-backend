using AirWatch.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirWatch.Infrastructure.Data.Configurations;

public class MeasurementConfiguration : IEntityTypeConfiguration<Measurement>
{
    public void Configure(EntityTypeBuilder<Measurement> builder)
    {
        builder.ToTable("measurements");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.SensorType).HasMaxLength(10).IsRequired();
        builder.Property(m => m.Calibrated).IsRequired();
        builder.Property(m => m.AdcRaw).IsRequired();
        builder.Property(m => m.VoltageV).IsRequired();
        builder.Property(m => m.RsOhm).IsRequired();
        builder.Property(m => m.RsR0Ratio).IsRequired();
        builder.Property(m => m.Ppm).IsRequired();
        builder.Property(m => m.Timestamp).IsRequired();

        builder.HasIndex(m => m.Timestamp);
        builder.HasIndex(m => m.SensorType);
    }
}
