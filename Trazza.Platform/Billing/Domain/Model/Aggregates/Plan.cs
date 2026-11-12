using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Aggregates.Internal;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Billing.Domain.Model.Aggregates;

/// <summary>
///     Plan of the catalog (table plans). It is read-only for the API: the rows are seeded in the database.
/// </summary>
public class Plan : AggregateRoot
{
    public const string RoutePerTruck = "ROUTE_PER_TRUCK";
    public const string IotPerTruck = "IOT_PER_TRUCK";
    public const string FullPack6 = "FULL_PACK_6";

    public Plan()
    {
        Code = string.Empty;
        Name = string.Empty;
        Price = new Money(0m, Money.Dollars);
    }

    public PlanId Id { get; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public EMonitoringType MonitoringType { get; private set; }
    public EPricingUnit PricingUnit { get; private set; }
    public Money Price { get; private set; }
    public EBillingPeriod BillingPeriod { get; private set; }
    public byte MinVehicles { get; private set; }
    public byte? MaxVehicles { get; private set; }
    public bool IsActive { get; private set; }

    public bool AcceptsVehicleCount(int vehicleCount) =>
        vehicleCount >= MinVehicles && (MaxVehicles is null || vehicleCount <= MaxVehicles);

    public Money CalculatePrice(int vehicleCount) =>
        PricingUnit == EPricingUnit.PerVehicle ? Price.Multiply(vehicleCount) : Price;
}
