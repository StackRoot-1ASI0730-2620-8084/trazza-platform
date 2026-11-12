using TrazzaPlatform.API.Iam.Application.Internal.OutboundServices;

namespace TrazzaPlatform.API.Iam.Infrastructure.Security;

/// <summary>
///     Simulated RENIEC check for the MVP: every DNI is validated except the test DNI <c>00000000</c>.
/// </summary>
public class SimulatedReniecProvider(TimeProvider timeProvider) : IIdentityVerificationProvider
{
    public const string RejectedTestDni = "00000000";

    public Task<(bool Approved, string Reference, string? Reason)> VerifyAsync(string dni, string fullName,
        CancellationToken cancellationToken = default)
    {
        var reference = $"RENIEC-SIM-{timeProvider.GetUtcNow():yyyyMMddHHmmss}-{dni[^3..]}";
        return Task.FromResult(dni == RejectedTestDni
            ? (false, reference, (string?)"dni-not-found-in-reniec")
            : (true, reference, (string?)null));
    }
}
