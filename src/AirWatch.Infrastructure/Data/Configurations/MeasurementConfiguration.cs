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

        builder.Property(m => m.Timestamp).IsRequired();
        builder.Property(m => m.Mq3Adc).IsRequired();
        builder.Property(m => m.Mq5Adc).IsRequired();
        builder.Property(m => m.Mq135Adc).IsRequired();
        builder.Property(m => m.PpmAlcohol).IsRequired();
        builder.Property(m => m.PpmLpg).IsRequired();
        builder.Property(m => m.PpmCo2).IsRequired();
        builder.Property(m => m.PpmNh3).IsRequired();

        builder.HasIndex(m => m.DeviceId);
        builder.HasIndex(m => m.Timestamp);
    }
}
