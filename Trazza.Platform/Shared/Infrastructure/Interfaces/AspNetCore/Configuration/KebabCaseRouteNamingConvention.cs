using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Trazza.Platform.Shared.Domain.Model.Extensions;

namespace Trazza.Platform.Shared.Infrastructure.Interfaces.AspNetCore.Configuration;

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
