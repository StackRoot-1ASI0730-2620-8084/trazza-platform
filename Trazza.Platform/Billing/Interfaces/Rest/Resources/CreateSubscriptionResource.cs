namespace Trazza.Platform.Billing.Interfaces.Rest.Resources;

public record CreateSubscriptionResource(int UserId, string Plan, SavePaymentMethodResource Method);
