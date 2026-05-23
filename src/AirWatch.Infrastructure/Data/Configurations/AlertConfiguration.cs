using AirWatch.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirWatch.Infrastructure.Data.Configurations;

public class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("alerts");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.DeviceId).IsRequired().HasColumnName("device_id");
        builder.Property(a => a.UserId).IsRequired().HasColumnName("user_id");
        builder.Property(a => a.TriggeredAt).IsRequired().HasColumnName("triggered_at");
        builder.Property(a => a.EmailSent).IsRequired().HasColumnName("email_sent");

        builder.HasIndex(a => a.DeviceId).HasDatabaseName("ix_alerts_device_id");
        builder.HasIndex(a => a.TriggeredAt).HasDatabaseName("ix_alerts_triggered_at");

        builder.HasOne(a => a.Device)
            .WithMany()
            .HasForeignKey(a => a.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
