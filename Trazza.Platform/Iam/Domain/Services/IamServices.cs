using Trazza.Platform.Iam.Domain.Model.Aggregates;
using Trazza.Platform.Iam.Domain.Model.Commands;
using Trazza.Platform.Shared.Domain.Model;

namespace Trazza.Platform.Iam.Domain.Services;

public record UserAccount(
    User User,
    PersonIdentity? Identity,
    string? Phone,
    CarrierProfile? CarrierProfile,
    MerchantProfile? MerchantProfile);

public record AuthenticatedSession(
    UserAccount Account,
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt);

public interface IUserCommandService
{
    Task<Result<AuthenticatedSession, CommandError>> Handle(SignUpCommand command, CancellationToken ct = default);

    Task<Result<AuthenticatedSession, CommandError>> Handle(SignInCommand command, CancellationToken ct = default);

    Task<Result<AuthenticatedSession, CommandError>> Handle(RefreshSessionCommand command, CancellationToken ct = default);

    Task<Result<bool, CommandError>> Handle(SignOutCommand command, CancellationToken ct = default);

    Task<Result<UserAccount, CommandError>> Handle(UpdateIdentityCommand command, CancellationToken ct = default);

    Task<Result<UserAccount, CommandError>> Handle(AddRoleCommand command, CancellationToken ct = default);

    Task<Result<UserAccount, CommandError>> Handle(UpdateMerchantProfileCommand command, CancellationToken ct = default);

    Task<Result<IdentityVerification, CommandError>> Handle(SubmitIdentityVerificationCommand command,
        CancellationToken ct = default);
}

public interface IUserQueryService
{
    Task<UserAccount?> GetAccountAsync(ulong userId, CancellationToken ct = default);

    Task<List<IdentityVerification>> ListVerificationsAsync(ulong userId, CancellationToken ct = default);
}

public interface IFleetCommandService
{
    Task<Result<Vehicle, CommandError>> Handle(CreateVehicleCommand command, CancellationToken ct = default);

    Task<Result<Vehicle, CommandError>> Handle(UpdateVehicleCommand command, CancellationToken ct = default);

    Task<Result<Vehicle, CommandError>> Handle(DeactivateVehicleCommand command, CancellationToken ct = default);

    Task<Result<Driver, CommandError>> Handle(CreateDriverCommand command, CancellationToken ct = default);

    Task<Result<Driver, CommandError>> Handle(UpdateDriverCommand command, CancellationToken ct = default);

    Task<Result<Driver, CommandError>> Handle(DeactivateDriverCommand command, CancellationToken ct = default);

    Task<Result<SavedAddress, CommandError>> Handle(CreateSavedAddressCommand command, CancellationToken ct = default);

    Task<Result<bool, CommandError>> Handle(DeleteSavedAddressCommand command, CancellationToken ct = default);
}

public interface IFleetQueryService
{
    Task<List<Vehicle>> ListVehiclesAsync(ulong userId, CancellationToken ct = default);

    Task<List<Driver>> ListDriversAsync(ulong userId, CancellationToken ct = default);

    Task<List<SavedAddress>> ListSavedAddressesAsync(ulong userId, CancellationToken ct = default);
}
