using Trazza.Platform.Billing.Domain.Model.ValueObjects;
 
namespace Trazza.Platform.Billing.Domain.Model.Aggregates;
 
/// <summary>
///     Monitoring plan sold by Trazza, priced in USD per truck or as a package (seeded in <c>plans</c>).
/// </summary>
public class Plan
{
    protected Plan()
    {
        Code = string.Empty;
        Name = string.Empty;
        Currency = string.Empty;
    }
 
    public ushort Id { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public MonitoringType MonitoringType { get; private set; }
    public PricingUnit PricingUnit { get; private set; }
    public decimal Price { get; private set; }
    public string Currency { get; private set; }
    public BillingPeriod BillingPeriod { get; private set; }
    public byte MinVehicles { get; private set; }
    public byte? MaxVehicles { get; private set; }
    public bool IsActive { get; private set; }
 
    /// <summary>IoT plans lend one cargo sensor per truck.</summary>
    public bool IncludesIot => MonitoringType is MonitoringType.IotCargo or MonitoringType.Full;
 
    public bool AcceptsVehicleCount(int count) => count >= MinVehicles && (MaxVehicles is null || count <= MaxVehicles);
 
    public decimal PriceFor(int vehicleCount) =>
        PricingUnit == PricingUnit.PerVehicle ? Price * vehicleCount : Price;
}