using Trazza.Platform.Shared.Domain.Repositories;
using Trazza.Platform.Shared.Infrastructure.Interfaces.AspNetCore.Configuration;
using Trazza.Platform.Shared.Infrastructure.Interfaces.AspNetCore.OpenApi;
using Trazza.Platform.Shared.Infrastructure.Mediator.Cortex.Configuration;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Interceptors;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Trazza.Platform.Shared.Infrastructure.Pipeline.Middleware.Components;
using Trazza.Platform.Shared.Resources;
using Trazza.Platform.Shared.Resources.Errors;
using Cortex.Mediator.Commands;
using Cortex.Mediator.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Scalar.AspNetCore;
using ProblemDetailsFactory = Trazza.Platform.Shared.Interfaces.Rest.ProblemDetails.ProblemDetailsFactory;

// Bounded context namespaces: uncomment each block when its feature branch is merged into develop.
// IAM (feature/iam)
// using Trazza.Platform.Iam.Application.Acl;
// using Trazza.Platform.Iam.Application.CommandServices;
// using Trazza.Platform.Iam.Application.Internal.CommandServices;
// using Trazza.Platform.Iam.Application.Internal.OutboundServices;
// using Trazza.Platform.Iam.Application.Internal.QueryServices;
// using Trazza.Platform.Iam.Application.QueryServices;
// using Trazza.Platform.Iam.Domain.Repositories;
// using Trazza.Platform.Iam.Infrastructure.Hashing.BCrypt.Services;
// using Trazza.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
// using Trazza.Platform.Iam.Infrastructure.Pipeline.Middleware.Extensions;
// using Trazza.Platform.Iam.Infrastructure.Tokens.Jwt.Configuration;
// using Trazza.Platform.Iam.Infrastructure.Tokens.Jwt.Services;
// using Trazza.Platform.Iam.Interfaces.Acl;
// Profiles (feature/profiles)
// using Trazza.Platform.Profiles.Application.Acl;
// using Trazza.Platform.Profiles.Application.CommandServices;
// using Trazza.Platform.Profiles.Application.Internal.CommandServices;
// using Trazza.Platform.Profiles.Application.Internal.QueryServices;
// using Trazza.Platform.Profiles.Application.QueryServices;
// using Trazza.Platform.Profiles.Domain.Repositories;
// using Trazza.Platform.Profiles.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
// using Trazza.Platform.Profiles.Interfaces.Acl;
// Matchmaking (feature/matchmaking)
using Trazza.Platform.Matchmaking.Application.Acl;
using Trazza.Platform.Matchmaking.Application.CommandServices;
using Trazza.Platform.Matchmaking.Application.Internal.CommandServices;
using Trazza.Platform.Matchmaking.Application.Internal.OutboundServices;
using Trazza.Platform.Matchmaking.Application.Internal.QueryServices;
using Trazza.Platform.Matchmaking.Application.QueryServices;
using Trazza.Platform.Matchmaking.Domain.Repositories;
using Trazza.Platform.Matchmaking.Domain.Services;
using Trazza.Platform.Matchmaking.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Trazza.Platform.Matchmaking.Infrastructure.Profiles;
using Trazza.Platform.Matchmaking.Infrastructure.Services;
using Trazza.Platform.Matchmaking.Interfaces.Acl;
// Execution (feature/execution)
// using Trazza.Platform.Execution.Application.Acl;
// using Trazza.Platform.Execution.Application.CommandServices;
// using Trazza.Platform.Execution.Application.Internal.CommandServices;
// using Trazza.Platform.Execution.Application.Internal.QueryServices;
// using Trazza.Platform.Execution.Application.QueryServices;
// using Trazza.Platform.Execution.Domain.Repositories;
// using Trazza.Platform.Execution.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
// using Trazza.Platform.Execution.Interfaces.Acl;
// Billing (feature/billing)
// using Trazza.Platform.Billing.Application.Acl;
// using Trazza.Platform.Billing.Application.CommandServices;
// using Trazza.Platform.Billing.Application.Internal.CommandServices;
// using Trazza.Platform.Billing.Application.Internal.OutboundServices;
// using Trazza.Platform.Billing.Application.Internal.QueryServices;
// using Trazza.Platform.Billing.Application.QueryServices;
// using Trazza.Platform.Billing.Domain.Repositories;
// using Trazza.Platform.Billing.Infrastructure.Payments.Simulated.Services;
// using Trazza.Platform.Billing.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
// using Trazza.Platform.Billing.Interfaces.Acl;
// Reputation (feature/reputation)
// using Trazza.Platform.Reputation.Application.CommandServices;
// using Trazza.Platform.Reputation.Application.Internal.CommandServices;
// using Trazza.Platform.Reputation.Application.Internal.QueryServices;
// using Trazza.Platform.Reputation.Application.QueryServices;
// using Trazza.Platform.Reputation.Domain.Repositories;
// using Trazza.Platform.Reputation.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

// Local development: load variables from the .env file at the repository root (never committed).
LoadDotEnvForDevelopment();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options => options.Conventions.Add(new KebabCaseRouteNamingConvention()))
    .AddDataAnnotationsLocalization();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("TrazzaWebAppPolicy",
        policy => policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var connectionStringTemplate = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionStringTemplate))
        throw new InvalidOperationException("Database connection string is not set in the configuration.");

    var connectionString = Environment.ExpandEnvironmentVariables(connectionStringTemplate);
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("Database connection string is not set in the configuration.");
    if (connectionString.Contains('%'))
        throw new InvalidOperationException(
            "Database connection string has unresolved environment variables. Set DATABASE_URL, DATABASE_USER, DATABASE_PASSWORD and DATABASE_SCHEMA.");

    options.UseMySQL(connectionString)
        .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
        .AddInterceptors(
            serviceProvider.GetRequiredService<AuditableEntityInterceptor>(),
            serviceProvider.GetRequiredService<DispatchDomainEventsInterceptor>())
        .EnableDetailedErrors();

    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging();
});

builder.Services.AddLocalization();

builder.Services.AddSingleton<IStringLocalizer<ErrorMessages>, StringLocalizer<ErrorMessages>>();
builder.Services.AddSingleton<IStringLocalizer<CommonMessages>, StringLocalizer<CommonMessages>>();

builder.Services.AddSingleton<ProblemDetailsFactory>();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    // IAM (feature/iam)
    // options.AddOperationTransformer<IamBearerSecurityOperationTransformer>();
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<AuditableEntityInterceptor>();
builder.Services.AddScoped<DispatchDomainEventsInterceptor>();

// Bounded context registrations: uncomment each block when its feature branch is merged into develop.
// IAM (feature/iam)
// builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));

// builder.Services.AddScoped<IUserRepository, UserRepository>();
// builder.Services.AddScoped<IUserCommandService, UserCommandService>();
// builder.Services.AddScoped<IUserQueryService, UserQueryService>();
// builder.Services.AddScoped<ITokenService, TokenService>();
// builder.Services.AddScoped<IHashingService, HashingService>();
// builder.Services.AddScoped<IIamContextFacade, IamContextFacade>();

// Profiles (feature/profiles)
// builder.Services.AddScoped<ICarrierProfileRepository, CarrierProfileRepository>();
// builder.Services.AddScoped<IMerchantProfileRepository, MerchantProfileRepository>();
// builder.Services.AddScoped<ICarrierProfileCommandService, CarrierProfileCommandService>();
// builder.Services.AddScoped<ICarrierProfileQueryService, CarrierProfileQueryService>();
// builder.Services.AddScoped<IMerchantProfileCommandService, MerchantProfileCommandService>();
// builder.Services.AddScoped<IMerchantProfileQueryService, MerchantProfileQueryService>();
// builder.Services.AddScoped<IProfilesContextFacade, ProfilesContextFacade>();

// Matchmaking (feature/matchmaking)
builder.Services.AddScoped<IReturnRouteRepository, ReturnRouteRepository>();
builder.Services.AddScoped<IShipmentRequestRepository, ShipmentRequestRepository>();
builder.Services.AddScoped<IMatchProposalRepository, MatchProposalRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IRouteMatchingService, RouteMatchingService>();
// Temporary: replace it with the adapter to the Profiles context when feature/profiles is merged.
builder.Services.AddScoped<IProfilesService, FakeProfilesService>();
builder.Services.AddScoped<IReturnRouteCommandService, ReturnRouteCommandService>();
builder.Services.AddScoped<IReturnRouteQueryService, ReturnRouteQueryService>();
builder.Services.AddScoped<IShipmentRequestCommandService, ShipmentRequestCommandService>();
builder.Services.AddScoped<IShipmentRequestQueryService, ShipmentRequestQueryService>();
builder.Services.AddScoped<IMatchProposalCommandService, MatchProposalCommandService>();
builder.Services.AddScoped<IMatchProposalQueryService, MatchProposalQueryService>();
builder.Services.AddScoped<IMessageCommandService, MessageCommandService>();
builder.Services.AddScoped<IMessageQueryService, MessageQueryService>();
builder.Services.AddScoped<IMatchmakingContextFacade, MatchmakingContextFacade>();

// Execution (feature/execution)
// builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
// builder.Services.AddScoped<IShipmentCommandService, ShipmentCommandService>();
// builder.Services.AddScoped<IShipmentQueryService, ShipmentQueryService>();
// builder.Services.AddScoped<IExecutionContextFacade, ExecutionContextFacade>();

// Billing (feature/billing)
// builder.Services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
// builder.Services.AddScoped<IReceiptRepository, ReceiptRepository>();
// builder.Services.AddScoped<IPaymentGateway, SimulatedPaymentGateway>();
// builder.Services.AddScoped<IPaymentTransactionCommandService, PaymentTransactionCommandService>();
// builder.Services.AddScoped<IPaymentTransactionQueryService, PaymentTransactionQueryService>();
// builder.Services.AddScoped<IReceiptCommandService, ReceiptCommandService>();
// builder.Services.AddScoped<IReceiptQueryService, ReceiptQueryService>();
// builder.Services.AddScoped<IBillingContextFacade, BillingContextFacade>();

// Reputation (feature/reputation)
// builder.Services.AddScoped<IRatingRepository, RatingRepository>();
// builder.Services.AddScoped<IRatingCommandService, RatingCommandService>();
// builder.Services.AddScoped<IRatingQueryService, RatingQueryService>();

builder.Services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(LoggingCommandBehavior<>));
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(ExceptionHandlingCommandBehavior<>));
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(TransactionCommandBehavior<>));

builder.Services.AddCortexMediator(
    [typeof(Program)]);

var app = builder.Build();

app.UseExceptionHandler();

var supportedCultures = new[] { "en", "es" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("TrazzaWebAppPolicy");

// IAM (feature/iam)
// app.UseRequestAuthorization();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static void LoadDotEnvForDevelopment()
{
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    if (!string.IsNullOrEmpty(environment) && !environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
        return;

    var envFile = FindDotEnvFile(Directory.GetCurrentDirectory()) ?? FindDotEnvFile(AppContext.BaseDirectory);
    if (envFile is null) return;

    foreach (var rawLine in File.ReadAllLines(envFile))
    {
        var line = rawLine.Trim();
        if (line.Length == 0 || line.StartsWith('#')) continue;

        var separator = line.IndexOf('=');
        if (separator <= 0) continue;

        var key = line[..separator].Trim();
        var value = line[(separator + 1)..].Trim().Trim('"');

        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
            Environment.SetEnvironmentVariable(key, value);
    }
}

static string? FindDotEnvFile(string startDirectory)
{
    var directory = new DirectoryInfo(startDirectory);
    while (directory is not null)
    {
        var candidate = Path.Combine(directory.FullName, ".env");
        if (File.Exists(candidate)) return candidate;
        directory = directory.Parent;
    }
    return null;
}
