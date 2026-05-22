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

        builder.Property(m => m.RatioMq3).IsRequired().HasDefaultValue(0.0);
        builder.Property(m => m.RatioMq5).IsRequired().HasDefaultValue(0.0);
        builder.Property(m => m.RatioMq135).IsRequired().HasDefaultValue(0.0);

        builder.Property(m => m.DegMq3).IsRequired().HasDefaultValue(0.0);
        builder.Property(m => m.DegMq5).IsRequired().HasDefaultValue(0.0);
        builder.Property(m => m.DegMq135).IsRequired().HasDefaultValue(0.0);

        builder.Property(m => m.Iqai).IsRequired().HasDefaultValue(0.0);
        builder.Property(m => m.IqaiCategory).IsRequired().HasMaxLength(20).HasDefaultValue("Boa");

        builder.HasIndex(m => m.DeviceId);
        builder.HasIndex(m => m.Timestamp);
    }
}
