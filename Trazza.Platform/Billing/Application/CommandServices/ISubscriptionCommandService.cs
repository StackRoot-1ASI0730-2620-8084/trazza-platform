using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.Commands;
using Trazza.Platform.Shared.Application.Model;

namespace Trazza.Platform.Billing.Application.CommandServices;

public interface ISubscriptionCommandService
{
    Task<Result<SubscriptionPayment>> Handle(SubscribeToPlanCommand command,
        CancellationToken cancellationToken);
}
