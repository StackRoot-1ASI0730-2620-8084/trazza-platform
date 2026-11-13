using System;
using Microsoft.EntityFrameworkCore;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.ValueConverters;
using Trazza.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Trazza.Platform.Matchmaking.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Trazza.Platform.Execution.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Trazza.Platform.Billing.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Trazza.Platform.Reputation.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

namespace Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Properties<DateTimeOffset>()
            .HaveConversion<UtcDateTimeOffsetConverter>()
            .HavePrecision(3);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyIamConfiguration();
        builder.ApplyMatchmakingConfiguration();
        builder.ApplyExecutionConfiguration();
        builder.ApplyBillingConfiguration();
        builder.ApplyReputationConfiguration();

        builder.UseSnakeCaseNamingConvention();
    }
}
