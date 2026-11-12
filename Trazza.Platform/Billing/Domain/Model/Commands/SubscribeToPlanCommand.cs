using Trazza.Platform.Billing.Domain.Model.ValueObjects;

namespace Trazza.Platform.Billing.Domain.Model.Commands;

public record SubscribeToPlanCommand(UserId UserId, string Plan, PaymentMethod Method);
