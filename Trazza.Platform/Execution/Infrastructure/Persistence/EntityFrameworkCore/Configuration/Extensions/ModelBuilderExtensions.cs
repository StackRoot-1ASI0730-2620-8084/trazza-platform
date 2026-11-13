using Microsoft.EntityFrameworkCore;
using Trazza.Platform.Execution.Domain.Model.Aggregates;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

namespace Trazza.Platform.Execution.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

/// <summary>
///     Mapping of the Service Execution &amp; Monitoring tables. <c>tracking_points</c> and <c>deviation_alerts</c>
///     have POINT columns and are handled with SQL.
/// </summary>
public static class ModelBuilderExtensions
{
    public static void ApplyExecutionConfiguration(this ModelBuilder builder)
    {
        builder.Entity<Trip>(entity =>
        {
            entity.ToTable("trips");
            entity.HasKey(trip => trip.Id);
            entity.Property(trip => trip.Id).ValueGeneratedOnAdd();
            entity.Property(trip => trip.PublicId).HasPublicId();
            entity.Property(trip => trip.AgreedRate).HasPrecision(10, 2);
            entity.Property(trip => trip.Currency).HasMaxLength(3).IsFixedLength();
            entity.Property(trip => trip.Status).HasEnumText();
            entity.Property(trip => trip.CancellationReason).HasMaxLength(255);
            entity.Property(trip => trip.TotalDistanceKm).HasPrecision(8, 2);
            entity.Ignore(trip => trip.IsClosed);
            entity.Ignore(trip => trip.IsOnTheRoad);
            entity.HasMany(trip => trip.History).WithOne().HasForeignKey(change => change.TripId);
            entity.Navigation(trip => trip.History).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Entity<TripStatusChange>(entity =>
        {
            entity.ToTable("trip_status_history");
            entity.HasKey(change => change.Id);
            entity.Property(change => change.Id).ValueGeneratedOnAdd();
            entity.Property(change => change.FromStatus).HasMaxLength(20);
            entity.Property(change => change.ToStatus).HasMaxLength(20).IsRequired();
        });

        builder.Entity<Incident>(entity =>
        {
            entity.ToTable("incidents");
            entity.HasKey(incident => incident.Id);
            entity.Property(incident => incident.Id).ValueGeneratedOnAdd();
            entity.Property(incident => incident.Type).HasEnumText();
            entity.Property(incident => incident.Status).HasEnumText();
            entity.Property(incident => incident.Description).HasMaxLength(1000).IsRequired();
            entity.Property(incident => incident.Resolution).HasMaxLength(1000);
            entity.HasMany(incident => incident.Evidences).WithOne().HasForeignKey(evidence => evidence.IncidentId);
            entity.Navigation(incident => incident.Evidences).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Entity<IncidentEvidence>(entity =>
        {
            entity.ToTable("incident_evidences");
            entity.HasKey(evidence => evidence.Id);
            entity.Property(evidence => evidence.Id).ValueGeneratedOnAdd();
            entity.Property(evidence => evidence.S3Key).HasColumnName("s3_key").HasMaxLength(512).IsRequired();
        });

        builder.Entity<IotDevice>(entity =>
        {
            entity.ToTable("iot_devices");
            entity.HasKey(device => device.Id);
            entity.Property(device => device.Id).ValueGeneratedOnAdd();
            entity.Property(device => device.SerialNumber).HasMaxLength(60).IsRequired();
            entity.Property(device => device.Status).HasEnumText();
            entity.Property(device => device.CreatedAt).ValueGeneratedOnAdd();
        });
    }
}
