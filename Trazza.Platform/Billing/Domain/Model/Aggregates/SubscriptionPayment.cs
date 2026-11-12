using Trazza.Platform.Billing.Domain.Model.Errors;
using Trazza.Platform.Billing.Domain.Model.Events;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Aggregates.Internal;
using Trazza.Platform.Shared.Domain.Model.Errors;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Billing.Domain.Model.Aggregates;

public partial class SubscriptionPayment : AggregateRoot
{
    public SubscriptionPayment()
    {
        Plan = new Plan();
        Amount = new Money();
        Method = new PaymentMethod();
    }

    public SubscriptionPayment(UserId userId, Plan plan, PaymentMethod method) : this()
    {
        UserId = userId;
        Plan = plan;
        Amount = plan.Price;
        Method = method;
        Status = ESubscriptionPaymentStatus.Pending;
    }

    public SubscriptionPaymentId Id { get; }
    public UserId UserId { get; private set; }
    public Plan Plan { get; private set; }
    public Money Amount { get; private set; }
    public PaymentMethod Method { get; private set; }
    public ESubscriptionPaymentStatus Status { get; private set; }
    public string? AuthorizationCode { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }
    public DateOnly? PeriodStart { get; private set; }
    public DateOnly? PeriodEnd { get; private set; }

    public bool IsPaid => Status == ESubscriptionPaymentStatus.Paid;

    public bool IsPending => Status == ESubscriptionPaymentStatus.Pending;

    public Error MarkPaid(string authorizationCode, DateTimeOffset now)
    {
        if (!IsPending) return BillingErrors.TransactionNotPending;
        if (string.IsNullOrWhiteSpace(authorizationCode)) return BillingErrors.AuthorizationRequired;

        var start = DateOnly.FromDateTime(now.ToOffset(LimaCalendar.UtcOffset).DateTime);
        Status = ESubscriptionPaymentStatus.Paid;
        AuthorizationCode = authorizationCode;
        PaidAt = now;
        PeriodStart = start;
        PeriodEnd = start.AddMonths(1).AddDays(-1);
        AddDomainEvent(new SubscriptionPaymentPaidEvent(UserId, Plan.Code, PeriodEnd.Value));
        return Error.None;
    }

    public Error MarkFailed()
    {
        if (!IsPending) return BillingErrors.TransactionNotPending;

        Status = ESubscriptionPaymentStatus.Failed;
        return Error.None;
    }

    public bool Covers(DateOnly today) =>
        IsPaid && PeriodStart <= today && today <= PeriodEnd;
}
