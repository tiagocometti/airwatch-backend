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
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.DeviceId).IsRequired().HasColumnName("device_id");
        builder.Property(c => c.StartedAt).IsRequired().HasColumnName("started_at");
        builder.Property(c => c.CompletedAt).HasColumnName("completed_at");

        builder.Property(c => c.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasColumnName("status");

        builder.Property(c => c.Location)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("location");

        builder.Property(c => c.R0Mq3).HasColumnName("r0_mq3");
        builder.Property(c => c.R0Mq5).HasColumnName("r0_mq5");
        builder.Property(c => c.R0Mq135).HasColumnName("r0_mq135");
        builder.Property(c => c.SampleCount).IsRequired().HasColumnName("sample_count");
        builder.Property(c => c.DuracaoSegundos).IsRequired().HasColumnName("duracao_segundos");
        builder.Property(c => c.IsActive).IsRequired().HasColumnName("is_active");

        builder.HasIndex(c => c.DeviceId).HasDatabaseName("ix_calibrations_device_id");
        builder.HasIndex(c => c.Status).HasDatabaseName("ix_calibrations_status");

        builder.HasOne(c => c.Device)
            .WithMany(d => d.Calibrations)
            .HasForeignKey(c => c.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
