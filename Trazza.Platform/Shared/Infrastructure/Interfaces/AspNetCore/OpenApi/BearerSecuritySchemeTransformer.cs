using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Trazza.Platform.Shared.Infrastructure.Interfaces.AspNetCore.OpenApi;

/// <summary>
///     Declares the JWT bearer security scheme in the OpenAPI document, so Scalar shows the authorization field.
///     The IAM context marks which operations require it.
/// </summary>
public class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public const string SchemeName = "Bearer";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info.Title = "Trazza Platform API";

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[SchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT obtained from POST /api/v1/authentication/sign-in. Send it as: Bearer {token}"
        };

        return Task.CompletedTask;
    }
}
