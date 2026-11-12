using Trazza.Platform.Billing.Application.CommandServices;
using Trazza.Platform.Billing.Application.Internal.OutboundServices;
using Trazza.Platform.Billing.Application.QueryServices;
using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.Commands;
using Trazza.Platform.Billing.Domain.Model.Errors;
using Trazza.Platform.Billing.Domain.Model.Queries;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Billing.Domain.Repositories;
using Trazza.Platform.Iam.Interfaces.Acl;
using Trazza.Platform.Shared.Application.Model;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Billing.Application.Internal.CommandServices;

public class SubscriptionCommandService(
    ISubscriptionPaymentRepository paymentTransactionRepository,
    ISubscriptionPaymentQueryService paymentTransactionQueryService,
    IPaymentGateway paymentGateway,
    IIamContextFacade iamContextFacade,
    IUnitOfWork unitOfWork)
    : ISubscriptionCommandService
{
    public async Task<Result<SubscriptionPayment>> Handle(SubscribeToPlanCommand command,
        CancellationToken cancellationToken)
    {
        var plan = Plan.From(command.Plan);
        if (!plan.IsValid)
            return Result<SubscriptionPayment>.Failure(BillingErrors.InvalidPlan);
        if (plan.IsFree)
            return Result<SubscriptionPayment>.Failure(BillingErrors.PlanFreeNotChargeable);
        if (!command.Method.IsValid)
            return Result<SubscriptionPayment>.Failure(BillingErrors.InvalidPaymentMethod);
        if (command.Method.IsExpired(LimaCalendar.Today))
            return Result<SubscriptionPayment>.Failure(BillingErrors.CardExpired);
        if (!await iamContextFacade.ExistsUserById(command.UserId, cancellationToken))
            return Result<SubscriptionPayment>.Failure(BillingErrors.UserNotFound);

        var activeSubscription = await paymentTransactionQueryService.Handle(
            new GetActiveSubscriptionsByUserIdQuery(command.UserId), cancellationToken);
        if (activeSubscription is not null)
            return Result<SubscriptionPayment>.Failure(BillingErrors.PlanAlreadyActive);

        var transaction = new SubscriptionPayment(command.UserId, plan, command.Method);
        var response = await paymentGateway.Charge(transaction.Amount, transaction.Method, cancellationToken);
        if (response.Approved)
            transaction.MarkPaid(response.AuthorizationCode ?? string.Empty, DateTimeOffset.UtcNow);
        else
            transaction.MarkFailed();

        await paymentTransactionRepository.AddAsync(transaction, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<SubscriptionPayment>.Success(transaction);
    }
}
