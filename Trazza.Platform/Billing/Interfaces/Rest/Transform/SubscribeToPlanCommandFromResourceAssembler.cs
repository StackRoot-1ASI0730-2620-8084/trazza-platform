using Trazza.Platform.Billing.Domain.Model.Commands;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Billing.Interfaces.Rest.Resources;

namespace Trazza.Platform.Billing.Interfaces.Rest.Transform;

public static class SubscribeToPlanCommandFromResourceAssembler
{
    public static SubscribeToPlanCommand ToCommandFromResource(CreateSubscriptionResource resource)
    {
        var vehicleIds = (resource.VehicleIds ?? []).Select(id => new VehicleId(id)).ToList();
        return new SubscribeToPlanCommand(
            new UserId(resource.UserId),
            (resource.PlanCode ?? string.Empty).Trim().ToUpperInvariant(),
            vehicleIds,
            resource.PaymentToken ?? string.Empty);
    }
}
