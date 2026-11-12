using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Billing.Interfaces.Rest.Resources;
using Trazza.Platform.Shared.Interfaces.Rest.Transform;

namespace Trazza.Platform.Billing.Interfaces.Rest.Transform;

public static class SubscriptionResourceFromEntityAssembler
{
    public static PlanResource ToResourceFromPlan(Plan plan)
    {
        return new PlanResource(plan.Code, MoneyResourceAssembler.ToResourceFromValueObject(plan.Price),
            plan.MonthlyPublications);
    }

    public static SubscriptionResource ToResourceFromActiveSubscription(int userId,
        SubscriptionPayment? activeSubscription)
    {
        var plan = activeSubscription?.Plan ?? new Plan(Plan.Free);
        return new SubscriptionResource(userId, plan.Code,
            MoneyResourceAssembler.ToResourceFromValueObject(plan.Price), plan.MonthlyPublications,
            activeSubscription?.PeriodStart, activeSubscription?.PeriodEnd);
    }
}
