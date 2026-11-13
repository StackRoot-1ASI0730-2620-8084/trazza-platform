using System;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Interfaces.Rest.Resources;

namespace Trazza.Platform.Shared.Interfaces.Rest.Transform;

public static class MoneyResourceAssembler
{
    public static MoneyResource ToResourceFromValueObject(Money money)
    {
        ArgumentNullException.ThrowIfNull(money);
        return new MoneyResource(money.Amount, money.Currency);
    }

    public static Money ToValueObjectFromResource(MoneyResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new Money(resource.Amount, resource.Currency);
    }
}
