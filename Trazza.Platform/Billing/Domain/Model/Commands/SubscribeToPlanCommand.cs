using Trazza.Platform.Billing.Domain.Model.ValueObjects;

namespace Trazza.Platform.Billing.Domain.Model.Commands;

/// <summary>
///     Subscribes a truck owner to a plan for the given vehicles and charges the first month.
/// </summary>
/// <param name="UserId">The user that buys the plan (owner of the trucks).</param>
/// <param name="PlanCode">Code of the plan, for example ROUTE_PER_TRUCK.</param>
/// <param name="VehicleIds">The trucks covered by the subscription.</param>
/// <param name="PaymentToken">Token from the front end for the simulated gateway (never card data).</param>
public record SubscribeToPlanCommand(
    UserId UserId,
    string PlanCode,
    IReadOnlyList<VehicleId> VehicleIds,
    string PaymentToken);
