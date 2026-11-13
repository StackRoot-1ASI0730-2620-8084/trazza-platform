using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Extensions;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.ValueConverters;

namespace Trazza.Platform.Matchmaking.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

/// <summary>
///     Maps the Matchmaking aggregates to the existing tables of trazza_database (database-first).
///     The POINT columns (origin_location, destination_location, pickup_location, delivery_location) are not
///     mapped: the repositories read and write them with SQL.
/// </summary>
public static class ModelBuilderExtensions
{
    public static void ApplyMatchmakingConfiguration(this ModelBuilder builder)
    {
        builder.Entity<ReturnRoute>(route =>
        {
            route.ToTable("return_routes");
            route.HasKey(r => r.Id);
            route.Property(r => r.Id)
                .HasConversion(new StronglyTypedIdConverter<ReturnRouteId, ulong>(id => id.Value,
                    value => new ReturnRouteId(value)))
                .ValueGeneratedOnAdd();
            route.Property(r => r.PublicId).HasPublicId();
            route.Property(r => r.CarrierProfileId)
                .HasConversion(new StronglyTypedIdConverter<CarrierProfileId, ulong>(id => id.Value,
                    value => new CarrierProfileId(value)))
                .IsRequired();
            route.Property(r => r.VehicleId)
                .HasConversion(new StronglyTypedIdConverter<VehicleId, ulong>(id => id.Value,
                    value => new VehicleId(value)))
                .IsRequired();
            route.Property(r => r.DriverId)
                .HasConversion(new StronglyTypedIdConverter<DriverId, ulong>(id => id.Value,
                    value => new DriverId(value)));
            route.ComplexProperty(r => r.Origin, origin =>
            {
                origin.Property(a => a.Address).HasColumnName("origin_address").HasMaxLength(255);
                origin.Ignore(a => a.Location);
            });
            route.ComplexProperty(r => r.Destination, destination =>
            {
                destination.Property(a => a.Address).HasColumnName("destination_address").HasMaxLength(255);
                destination.Ignore(a => a.Location);
            });
            route.Property(r => r.DepartureAt).HasColumnName("departure_at").IsRequired();
            route.Property(r => r.MaxShipments).HasColumnName("max_shipments").IsRequired();
            route.Property(r => r.Status).HasConversion(EnumConverter<EReturnRouteStatus>()).HasMaxLength(20)
                .IsRequired();
            route.Property(r => r.CreatedAt).HasColumnName("created_at");
            route.Property(r => r.UpdatedAt).HasColumnName("updated_at");
        });

        builder.Entity<ShipmentRequest>(request =>
        {
            request.ToTable("shipment_requests");
            request.HasKey(r => r.Id);
            request.Property(r => r.Id)
                .HasConversion(new StronglyTypedIdConverter<ShipmentRequestId, ulong>(id => id.Value,
                    value => new ShipmentRequestId(value)))
                .ValueGeneratedOnAdd();
            request.Property(r => r.PublicId).HasPublicId();
            request.Property(r => r.MerchantProfileId)
                .HasConversion(new StronglyTypedIdConverter<MerchantProfileId, ulong>(id => id.Value,
                    value => new MerchantProfileId(value)))
                .IsRequired();
            request.Property(r => r.Description).HasMaxLength(500).IsRequired();
            request.ComplexProperty(r => r.Cargo, cargo =>
            {
                cargo.Property(c => c.WeightKg).HasColumnName("weight_kg").HasPrecision(10, 2);
                cargo.Property(c => c.VolumeM3).HasColumnName("volume_m3").HasPrecision(7, 2);
            });
            request.ComplexProperty(r => r.Pickup, pickup =>
            {
                pickup.Property(a => a.Address).HasColumnName("pickup_address").HasMaxLength(255);
                pickup.Ignore(a => a.Location);
            });
            request.ComplexProperty(r => r.Delivery, delivery =>
            {
                delivery.Property(a => a.Address).HasColumnName("delivery_address").HasMaxLength(255);
                delivery.Ignore(a => a.Location);
            });
            request.ComplexProperty(r => r.PickupWindow, window =>
            {
                window.Property(w => w.From).HasColumnName("pickup_from");
                window.Property(w => w.Until).HasColumnName("pickup_until");
            });
            request.Property(r => r.Status).HasConversion(EnumConverter<EShipmentRequestStatus>()).HasMaxLength(20)
                .IsRequired();
            request.Property(r => r.CancelledAt).HasColumnName("cancelled_at");
            request.Property(r => r.CancellationReason).HasColumnName("cancellation_reason").HasMaxLength(255);
            request.Property(r => r.CreatedAt).HasColumnName("created_at");
            request.Property(r => r.UpdatedAt).HasColumnName("updated_at");
        });

        builder.Entity<MatchProposal>(proposal =>
        {
            proposal.ToTable("match_proposals");
            proposal.HasKey(p => p.Id);
            proposal.Property(p => p.Id)
                .HasConversion(new StronglyTypedIdConverter<MatchProposalId, ulong>(id => id.Value,
                    value => new MatchProposalId(value)))
                .ValueGeneratedOnAdd();
            proposal.Property(p => p.PublicId).HasPublicId();
            proposal.Property(p => p.ShipmentRequestId)
                .HasConversion(new StronglyTypedIdConverter<ShipmentRequestId, ulong>(id => id.Value,
                    value => new ShipmentRequestId(value)))
                .IsRequired();
            proposal.Property(p => p.ReturnRouteId)
                .HasConversion(new StronglyTypedIdConverter<ReturnRouteId, ulong>(id => id.Value,
                    value => new ReturnRouteId(value)))
                .IsRequired();
            proposal.HasIndex(p => new { p.ShipmentRequestId, p.ReturnRouteId }).IsUnique();
            proposal.ComplexProperty(p => p.OfferedRate, rate =>
            {
                rate.Property(m => m.Amount).HasColumnName("offered_rate").HasPrecision(10, 2);
                rate.Property(m => m.Currency).HasColumnName("currency").HasMaxLength(3);
            });
            proposal.Property(p => p.Status).HasConversion(EnumConverter<EMatchProposalStatus>()).HasMaxLength(20)
                .IsRequired();
            proposal.Property(p => p.RespondedAt).HasColumnName("responded_at");
            proposal.Property(p => p.CreatedAt).HasColumnName("created_at");
            proposal.Property(p => p.UpdatedAt).HasColumnName("updated_at");
        });

        builder.Entity<Message>(message =>
        {
            message.ToTable("messages");
            message.HasKey(m => m.Id);
            message.Property(m => m.Id)
                .HasConversion(new StronglyTypedIdConverter<MessageId, ulong>(id => id.Value,
                    value => new MessageId(value)))
                .ValueGeneratedOnAdd();
            message.Property(m => m.MatchProposalId)
                .HasConversion(new StronglyTypedIdConverter<MatchProposalId, ulong>(id => id.Value,
                    value => new MatchProposalId(value)))
                .IsRequired();
            message.HasIndex(m => new { m.MatchProposalId, m.SentAt });
            message.Property(m => m.SenderUserId)
                .HasConversion(new StronglyTypedIdConverter<UserId, ulong>(id => id.Value,
                    value => new UserId(value)))
                .IsRequired();
            message.Property(m => m.Content).HasMaxLength(2000).IsRequired();
            message.Property(m => m.SentAt).HasColumnName("sent_at");
            message.Property(m => m.ReadAt).HasColumnName("read_at");
        });
    }

    /// <summary>
    ///     The database stores the enums as UPPER_SNAKE text (IN_PROGRESS).
    /// </summary>
    private static ValueConverter<TEnum, string> EnumConverter<TEnum>() where TEnum : struct, Enum
    {
        return new ValueConverter<TEnum, string>(value => value.ToSnakeCaseValue().ToUpperInvariant(),
            text => ParseEnum<TEnum>(text));
    }

    private static TEnum ParseEnum<TEnum>(string text) where TEnum : struct, Enum
    {
        return text.TryParseSnakeCase<TEnum>(out var value)
            ? value
            : throw new InvalidOperationException($"'{text}' is not a valid {typeof(TEnum).Name}.");
    }
}
