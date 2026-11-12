using TrazzaPlatform.API.Billing.Domain.Model.Aggregates;
using TrazzaPlatform.API.Billing.Domain.Repositories;
 
namespace TrazzaPlatform.API.Billing.Interfaces.Acl;
 
public class BillingContextFacade(
    IFreightSettlementRepository settlementRepository,
    ICancellationCompensationRepository compensationRepository) : IBillingContextFacade
{
    public async Task CreateFreightSettlementAsync(ulong tripId, decimal amount, string currency,
        CancellationToken ct = default)
    {
        if (await settlementRepository.FindByIdAsync(tripId, ct) is not null) return;
        await settlementRepository.AddAsync(new FreightSettlement(tripId, amount, currency), ct);
    }
 
    public async Task CreateCancellationCompensationAsync(ulong tripId, decimal amount, string currency, DateTime now,
        CancellationToken ct = default)
    {
        if (amount <= 0 || await compensationRepository.FindByIdAsync(tripId, ct) is not null) return;
        await compensationRepository.AddAsync(new CancellationCompensation(tripId, amount, currency, now), ct);
    }
}