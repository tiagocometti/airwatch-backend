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

        builder.Property(m => m.GasValue).IsRequired();
        builder.Property(m => m.Temperature).IsRequired();
        builder.Property(m => m.Humidity).IsRequired();

        builder.Property(m => m.Timestamp).IsRequired();

        builder.HasIndex(m => m.Timestamp);
        builder.HasIndex(m => m.SensorId);
    }
}
