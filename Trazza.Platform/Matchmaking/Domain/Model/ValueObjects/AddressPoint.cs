namespace Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

public record AddressPoint(string Address, GeoLocation Location)
{
    public const int MaxAddressLength = 255;

    public AddressPoint() : this(string.Empty, new GeoLocation())
    {
    }

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(Address) && Address.Length <= MaxAddressLength && Location.IsValid;
}
