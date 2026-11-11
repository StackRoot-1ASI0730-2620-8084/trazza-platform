using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Trazza.Platform.Shared.Interfaces.Rest;

/// <summary>
/// Health check: confirms the API is running and can reach the database.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class HealthController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetHealth(CancellationToken cancellationToken)
    {
        var databaseReachable = await context.Database.CanConnectAsync(cancellationToken);

        var body = new
        {
            status = databaseReachable ? "healthy" : "unhealthy",
            database = databaseReachable ? "connected" : "unreachable",
            checkedAt = DateTimeOffset.UtcNow
        };

        return databaseReachable ? Ok(body) : StatusCode(StatusCodes.Status503ServiceUnavailable, body);
    }
}
