using Microsoft.EntityFrameworkCore;
using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
 
namespace Trazza.Platform.Billing.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyBillingConfiguration(this ModelBuilder builder)
    {
        builder.Entity<Plan>(entity =>
        {
            entity.ToTable("plans");
            entity.HasKey(plan => plan.Id);
            entity.Property(plan => plan.Code).HasMaxLength(30).IsRequired();
            entity.Property(plan => plan.Name).HasMaxLength(100).IsRequired();
            entity.Property(plan => plan.MonitoringType).HasEnumText();
            entity.Property(plan => plan.PricingUnit).HasEnumText();
            entity.Property(plan => plan.BillingPeriod).HasEnumText();
            entity.Property(plan => plan.Price).HasPrecision(10, 2);
            entity.Property(plan => plan.Currency).HasMaxLength(3).IsFixedLength();
            entity.Ignore(plan => plan.IncludesIot);
        });
 
        builder.Entity<Subscription>(entity =>
        {
            entity.ToTable("subscriptions");
            entity.HasKey(subscription => subscription.Id);
            entity.Property(subscription => subscription.Id).ValueGeneratedOnAdd();
            entity.Property(subscription => subscription.Status).HasEnumText();
            entity.Property(subscription => subscription.GatewaySubscriptionId).HasMaxLength(100);
            entity.Ignore(subscription => subscription.IsActive);
            entity.HasMany(subscription => subscription.Vehicles).WithOne()
                .HasForeignKey(vehicle => vehicle.SubscriptionId);
            entity.Navigation(subscription => subscription.Vehicles).UsePropertyAccessMode(PropertyAccessMode.Field);
        });
 
        builder.Entity<SubscriptionVehicle>(entity =>
        {
            entity.ToTable("subscription_vehicles");
            entity.HasKey(vehicle => new { vehicle.SubscriptionId, vehicle.VehicleId });
        });
 
        builder.Entity<SubscriptionPayment>(entity =>
        {
            entity.ToTable("subscription_payments");
            entity.HasKey(payment => payment.Id);
            entity.Property(payment => payment.Id).ValueGeneratedOnAdd();
            entity.Property(payment => payment.Amount).HasPrecision(10, 2);
            entity.Property(payment => payment.Currency).HasMaxLength(3).IsFixedLength();
            entity.Property(payment => payment.Status).HasEnumText();
            entity.Property(payment => payment.GatewayPaymentId).HasMaxLength(100);
        });
 
        builder.Entity<FreightSettlement>(entity =>
        {
            entity.ToTable("freight_settlements");
            entity.HasKey(settlement => settlement.TripId);
            entity.Property(settlement => settlement.TripId).ValueGeneratedNever();
            entity.Property(settlement => settlement.Amount).HasPrecision(10, 2);
            entity.Property(settlement => settlement.Currency).HasMaxLength(3).IsFixedLength();
            entity.Property(settlement => settlement.ExternalReference).HasMaxLength(100);
            entity.Ignore(settlement => settlement.Status);
        });
 
        builder.Entity<CancellationCompensation>(entity =>
        {
            entity.ToTable("cancellation_compensations");
            entity.HasKey(compensation => compensation.TripId);
            entity.Property(compensation => compensation.TripId).ValueGeneratedNever();
            entity.Property(compensation => compensation.Amount).HasPrecision(10, 2);
            entity.Property(compensation => compensation.Currency).HasMaxLength(3).IsFixedLength();
            entity.Property(compensation => compensation.Status).HasEnumText();
            entity.Ignore(compensation => compensation.IsOpen);
        });
    }
}
