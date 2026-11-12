namespace Trazza.Platform.Billing.Interfaces.Rest.Resources;

/// <summary>
///     Body to subscribe to a plan. UserId will come from the JWT once IAM is merged.
/// </summary>
public record CreateSubscriptionResource(
    ulong UserId,
    string PlanCode,
    IReadOnlyList<ulong> VehicleIds,
    string PaymentToken);
