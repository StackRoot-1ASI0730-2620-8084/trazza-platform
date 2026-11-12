using Trazza.Platform.Shared.Interfaces.Rest.Resources;

namespace Trazza.Platform.Billing.Interfaces.Rest.Resources;

public record PlanResource(string Code, MoneyResource Price, int? MonthlyPublications);
