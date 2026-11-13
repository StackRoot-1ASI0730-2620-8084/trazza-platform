using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.Commands;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Billing.Domain.Repositories;
using Trazza.Platform.Billing.Domain.Services;
using Trazza.Platform.Execution.Interfaces.Acl;
using Trazza.Platform.Iam.Interfaces.Acl;
using Trazza.Platform.Shared.Domain.Model;
using Trazza.Platform.Shared.Domain.Repositories;
 
namespace Trazza.Platform.Billing.Application.Internal.CommandServices;
 
public class SubscriptionCommandService(
    IPlanRepository planRepository,
    ISubscriptionRepository subscriptionRepository,
    ISubscriptionPaymentRepository paymentRepository,
    IIamContextFacade iamContextFacade,
    IExecutionContextFacade executionContextFacade,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    SubscriptionViewBuilder viewBuilder) : ISubscriptionCommandService
{
    public Task<Result<SubscriptionView, CommandError>> Handle(SubscribeCommand command, CancellationToken ct = default) =>
        unitOfWork.ExecuteInTransactionAsync(() => SubscribeAsync(command, ct), result => result.IsSuccess, ct);
 
    public async Task<Result<SubscriptionView, CommandError>> Handle(RenewSubscriptionCommand command,
        CancellationToken ct = default)
    {
        var subscription = await subscriptionRepository.FindWithVehiclesAsync(command.SubscriptionId, ct);
        if (subscription is null || subscription.UserId != command.UserId)
            return CommandError.NotFound("subscription-not-found");
        if (subscription.Status == SubscriptionStatus.Cancelled)
            return CommandError.Conflict("subscription-cancelled");
 
        var plan = (await planRepository.ListAllAsync(ct)).First(item => item.Id == subscription.PlanId);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        await paymentRepository.AddAsync(SubscriptionPayment.Simulated(subscription.Id,
            plan.PriceFor(subscription.Vehicles.Count), plan.Currency, now), ct);
        subscription.Renew();
        await unitOfWork.CompleteAsync(ct);
        return await viewBuilder.BuildAsync(subscription, ct);
    }
 
    public async Task<Result<SubscriptionView, CommandError>> Handle(CancelSubscriptionCommand command,
        CancellationToken ct = default)
    {
        var subscription = await subscriptionRepository.FindWithVehiclesAsync(command.SubscriptionId, ct);
        if (subscription is null || subscription.UserId != command.UserId)
            return CommandError.NotFound("subscription-not-found");
        if (subscription.Status == SubscriptionStatus.Cancelled)
            return CommandError.Conflict("subscription-cancelled");
 
        var plan = (await planRepository.ListAllAsync(ct)).First(item => item.Id == subscription.PlanId);
        subscription.Cancel(timeProvider.GetUtcNow().UtcDateTime);
        if (plan.IncludesIot)
            await executionContextFacade.ReturnIotDevicesAsync(
                subscription.Vehicles.Select(vehicle => vehicle.VehicleId).ToList(), ct);
        await unitOfWork.CompleteAsync(ct);
        return await viewBuilder.BuildAsync(subscription, ct);
    }
 
    private async Task<Result<SubscriptionView, CommandError>> SubscribeAsync(SubscribeCommand command,
        CancellationToken ct)
    {
        var carrierProfileId = await iamContextFacade.FindCarrierProfileIdAsync(command.UserId, ct);
        if (carrierProfileId is null) return CommandError.Forbidden("carrier-profile-required");
 
        var plan = await planRepository.FindByCodeAsync(command.PlanCode?.Trim().ToUpperInvariant() ?? string.Empty, ct);
        if (plan is null || !plan.IsActive) return CommandError.NotFound("plan-not-found");
 
        var requested = (command.VehicleIds ?? []).Distinct().ToList();
        if (!plan.AcceptsVehicleCount(requested.Count)) return CommandError.Validation("plan-vehicle-count-invalid");
 
        var vehicles = await iamContextFacade.ListVehiclesByPublicIdsAsync(requested, ct);
        if (vehicles.Count != requested.Count || vehicles.Any(vehicle => vehicle.CarrierProfileId != carrierProfileId))
            return CommandError.NotFound("vehicle-not-found");
        if (vehicles.Any(vehicle => !vehicle.IsActive)) return CommandError.Conflict("vehicle-inactive");
 
        var vehicleIds = vehicles.Select(vehicle => vehicle.Id).ToList();
        if ((await subscriptionRepository.ListSubscribedVehicleIdsAsync(vehicleIds, ct)).Count > 0)
            return CommandError.Conflict("vehicle-already-subscribed");
 
        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (plan.IncludesIot && !await executionContextFacade.LoanIotDevicesAsync(vehicleIds, now, ct))
            return CommandError.Conflict("iot-device-unavailable");
 
        var subscription = new Subscription(command.UserId, plan.Id, vehicleIds, $"SIM-SUB-{Guid.CreateVersion7():N}", now);
        await subscriptionRepository.AddAsync(subscription, ct);
        await unitOfWork.CompleteAsync(ct);
 
        await paymentRepository.AddAsync(SubscriptionPayment.Simulated(subscription.Id, plan.PriceFor(vehicleIds.Count),
            plan.Currency, now), ct);
        await unitOfWork.CompleteAsync(ct);
        return await viewBuilder.BuildAsync(subscription, ct);
    }
}
