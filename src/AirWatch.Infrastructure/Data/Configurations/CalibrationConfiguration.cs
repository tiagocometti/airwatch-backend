using AirWatch.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirWatch.Infrastructure.Data.Configurations;

public class CalibrationConfiguration : IEntityTypeConfiguration<Calibration>
{
    public void Configure(EntityTypeBuilder<Calibration> builder)
    {
        builder.ToTable("calibrations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.DeviceId).IsRequired();
        builder.Property(c => c.StartedAt).IsRequired();
        builder.Property(c => c.CompletedAt);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(c => c.Location)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(c => c.R0Mq3);
        builder.Property(c => c.R0Mq5);
        builder.Property(c => c.R0Mq135);
        builder.Property(c => c.SampleCount).IsRequired();
        builder.Property(c => c.DuracaoSegundos).IsRequired();
        builder.Property(c => c.IsActive).IsRequired();

        builder.HasIndex(c => c.DeviceId).HasDatabaseName("ix_calibrations_device_id");
        builder.HasIndex(c => c.Status).HasDatabaseName("ix_calibrations_status");

        builder.HasOne(c => c.Device)
            .WithMany(d => d.Calibrations)
            .HasForeignKey(c => c.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
