using System;
using Microsoft.EntityFrameworkCore;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.ValueConverters;

// Bounded context configurations: uncomment each block when its feature branch is merged into develop.
// using Trazza.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
// using Trazza.Platform.Profiles.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Trazza.Platform.Matchmaking.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
// using Trazza.Platform.Execution.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
// using Trazza.Platform.Billing.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
// using Trazza.Platform.Reputation.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

namespace Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
///     Single EF Core context of the platform, mapped to the existing trazza_database schema (database-first).
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // trazza_database stores timestamps as DATETIME(3) in UTC.
        configurationBuilder.Properties<DateTimeOffset>()
            .HaveConversion<UtcDateTimeOffsetConverter>()
            .HavePrecision(3);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // builder.ApplyIamConfiguration();
        // builder.ApplyProfilesConfiguration();
        builder.ApplyMatchmakingConfiguration();
        // builder.ApplyExecutionConfiguration();
        // builder.ApplyBillingConfiguration();
        // builder.ApplyReputationConfiguration();

        builder.UseSnakeCaseNamingConvention();
    }
}
