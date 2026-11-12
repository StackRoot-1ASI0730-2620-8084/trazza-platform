namespace Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

/// <summary>
///     A text address together with its coordinates.
/// </summary>
public record AddressPoint(string Address, GeoLocation Location)
{
    public const int MaxAddressLength = 255;

    public AddressPoint() : this(string.Empty, new GeoLocation())
    {
    }

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(Address) && Address.Length <= MaxAddressLength && Location.IsValid;
}
