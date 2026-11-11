using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Trazza.Platform.Shared.Domain.Model.Extensions;

namespace Trazza.Platform.Shared.Infrastructure.Interfaces.AspNetCore.Configuration;

/// <summary>
///     Replaces the [controller] route token with the kebab-case controller name.
///     Example: PaymentTransactionsController with [Route("api/v1/[controller]")] is served at api/v1/payment-transactions.
/// </summary>
public class KebabCaseRouteNamingConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        var kebabCaseName = controller.ControllerName.ToSnakeCase().Replace('_', '-');

        foreach (var selector in controller.Selectors)
        {
            if (selector.AttributeRouteModel?.Template is not { } template) continue;

            selector.AttributeRouteModel.Template = template.Replace("[controller]", kebabCaseName);
        }
    }
}
