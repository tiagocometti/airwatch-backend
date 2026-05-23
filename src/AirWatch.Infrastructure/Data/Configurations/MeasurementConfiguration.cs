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
        builder.Property(m => m.Id).HasColumnName("id");

        builder.Property(m => m.DeviceId).IsRequired().HasColumnName("device_id");
        builder.Property(m => m.Timestamp).IsRequired().HasColumnName("timestamp");
        builder.Property(m => m.Mq3Adc).IsRequired().HasColumnName("mq3_adc");
        builder.Property(m => m.Mq5Adc).IsRequired().HasColumnName("mq5_adc");
        builder.Property(m => m.Mq135Adc).IsRequired().HasColumnName("mq135_adc");

        builder.Property(m => m.RatioMq3).IsRequired().HasDefaultValue(0.0).HasColumnName("ratio_mq3");
        builder.Property(m => m.RatioMq5).IsRequired().HasDefaultValue(0.0).HasColumnName("ratio_mq5");
        builder.Property(m => m.RatioMq135).IsRequired().HasDefaultValue(0.0).HasColumnName("ratio_mq135");

        builder.Property(m => m.DegMq3).IsRequired().HasDefaultValue(0.0).HasColumnName("deg_mq3");
        builder.Property(m => m.DegMq5).IsRequired().HasDefaultValue(0.0).HasColumnName("deg_mq5");
        builder.Property(m => m.DegMq135).IsRequired().HasDefaultValue(0.0).HasColumnName("deg_mq135");

        builder.Property(m => m.Iqai).IsRequired().HasDefaultValue(0.0).HasColumnName("iqai");
        builder.Property(m => m.IqaiCategory).IsRequired().HasMaxLength(20).HasDefaultValue("Boa").HasColumnName("iqai_category");

        builder.HasIndex(m => m.DeviceId);
        builder.HasIndex(m => m.Timestamp);
    }
}
