using Trazza.Platform.Billing.Domain.Model.ValueObjects;
 
namespace Trazza.Platform.Billing.Domain.Model.Aggregates;

public class FreightSettlement
{
    protected FreightSettlement()
    {
        Currency = string.Empty;
    }
 
    public FreightSettlement(ulong tripId, decimal amount, string currency)
    {
        TripId = tripId;
        Amount = amount;
        Currency = currency;
    }
 
    public ulong TripId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public DateTime? MerchantMarkedPaidAt { get; private set; }
    public DateTime? CarrierConfirmedAt { get; private set; }
    public string? ExternalReference { get; private set; }
 
    public SettlementStatus Status =>
        CarrierConfirmedAt.HasValue ? SettlementStatus.Settled
        : MerchantMarkedPaidAt.HasValue ? SettlementStatus.MarkedPaid
        : SettlementStatus.Pending;
 
    public void MarkPaid(string? externalReference, DateTime now)
    {
        MerchantMarkedPaidAt ??= now;
        if (!string.IsNullOrWhiteSpace(externalReference)) ExternalReference = externalReference.Trim();
    }
 
    public void ConfirmReceived(DateTime now) => CarrierConfirmedAt ??= now;
}

public class CancellationCompensation
{
    protected CancellationCompensation()
    {
        Currency = string.Empty;
    }
 
    public CancellationCompensation(ulong tripId, decimal amount, string currency, DateTime now)
    {
        TripId = tripId;
        Amount = amount;
        Currency = currency;
        Status = CompensationStatus.Pending;
        CreatedAt = now;
    }
 
    public ulong TripId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public CompensationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? SettledAt { get; private set; }
 
    public bool IsOpen => Status is CompensationStatus.Pending or CompensationStatus.Disputed;
 
    public void ConfirmPaid(DateTime now) => Settle(CompensationStatus.PaidExternally, now);
 
    public void Waive(DateTime now) => Settle(CompensationStatus.Waived, now);
 
    public void Dispute()
    {
        if (Status == CompensationStatus.Pending) Status = CompensationStatus.Disputed;
    }
 
    private void Settle(CompensationStatus status, DateTime now)
    {
        if (!IsOpen) return;
        Status = status;
        SettledAt = now;
    }
}
