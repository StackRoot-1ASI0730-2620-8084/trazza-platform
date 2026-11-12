namespace Trazza.Platform.Billing.Interfaces.Acl;

public interface IBillingContextFacade
{
    Task CreateFreightSettlementAsync(ulong tripId, decimal amount, string currency, CancellationToken ct = default);
 
    Task CreateCancellationCompensationAsync(ulong tripId, decimal amount, string currency, DateTime now,
        CancellationToken ct = default);
}