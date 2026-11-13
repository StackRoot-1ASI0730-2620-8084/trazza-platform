namespace Trazza.Platform.Billing.Domain.Model.Commands;
 
public record SubscribeCommand(ulong UserId, string PlanCode, IReadOnlyCollection<Guid> VehicleIds);
 
public record RenewSubscriptionCommand(ulong UserId, ulong SubscriptionId);
 
public record CancelSubscriptionCommand(ulong UserId, ulong SubscriptionId);
 
public record MarkFreightPaidCommand(ulong UserId, Guid TripId, string? ExternalReference);
 
public record ConfirmFreightReceivedCommand(ulong UserId, Guid TripId);
 
public enum CompensationAction
{
    ConfirmPaid,
    Waive,
    Dispute
}
 
public record UpdateCompensationCommand(ulong UserId, Guid TripId, CompensationAction Action);