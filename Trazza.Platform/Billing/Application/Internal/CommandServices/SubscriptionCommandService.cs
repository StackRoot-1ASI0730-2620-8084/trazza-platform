using Trazza.Platform.Billing.Application.CommandServices;
using Trazza.Platform.Billing.Application.Internal.OutboundServices;
using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.Commands;
using Trazza.Platform.Billing.Domain.Model.Errors;
using Trazza.Platform.Billing.Domain.Repositories;
using Trazza.Platform.Shared.Application.Model;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Billing.Application.Internal.CommandServices;

public class SubscriptionCommandService(
    IPlanRepository planRepository,
    ISubscriptionRepository subscriptionRepository,
    ISubscriptionPaymentRepository subscriptionPaymentRepository,
    IPaymentGateway paymentGateway,
    IIamUserService iamUserService,
    IFleetService fleetService,
    IUnitOfWork unitOfWork)
    : ISubscriptionCommandService
{
    public async Task<Result<Subscription>> Handle(SubscribeToPlanCommand command,
        CancellationToken cancellationToken)
    {
        var plan = await planRepository.FindByCodeAsync(command.PlanCode, cancellationToken);
        if (plan is null) return BillingErrors.PlanNotFound;
        if (!plan.IsActive) return BillingErrors.PlanInactive;

        var vehicleIds = command.VehicleIds.Distinct().ToList();
        if (!plan.AcceptsVehicleCount(vehicleIds.Count)) return BillingErrors.InvalidVehicleCount;

        if (!await iamUserService.ExistsUserById(command.UserId, cancellationToken))
            return BillingErrors.UserNotFound;
        if (!await fleetService.OwnsVehicles(command.UserId, vehicleIds, cancellationToken))
            return BillingErrors.VehicleNotOwned;

        foreach (var vehicleId in vehicleIds)
            if (await subscriptionRepository.IsVehicleCoveredAsync(vehicleId, plan.MonitoringType, cancellationToken))
                return BillingErrors.VehicleAlreadyCovered;

        var amount = plan.CalculatePrice(vehicleIds.Count);
        var response = await paymentGateway.Charge(amount, command.PaymentToken, cancellationToken);
        if (!response.Approved || string.IsNullOrWhiteSpace(response.GatewayPaymentId))
            return BillingErrors.PaymentDeclined;

        var now = DateTimeOffset.UtcNow;
        var subscription = new Subscription(command.UserId, plan.Id, vehicleIds, now);
        await subscriptionRepository.AddAsync(subscription, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        var payment = new SubscriptionPayment(subscription.Id, amount);
        payment.MarkPaid(response.GatewayPaymentId, now);
        await subscriptionPaymentRepository.AddAsync(payment, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return subscription;
    }
}
