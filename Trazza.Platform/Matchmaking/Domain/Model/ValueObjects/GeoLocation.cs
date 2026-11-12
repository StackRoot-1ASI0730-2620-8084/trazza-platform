namespace Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

/// <summary>
///     A point on Earth in WGS 84 (SRID 4326). It maps to the POINT columns of the database.
/// </summary>
public record GeoLocation(double Latitude, double Longitude)
{
    private const double EarthRadiusKm = 6371.0088;

    public GeoLocation() : this(0, 0)
    {
    }

    public bool IsValid => Latitude is >= -90 and <= 90 && Longitude is >= -180 and <= 180;

    /// <summary>
    ///     Great-circle distance in kilometers (haversine formula).
    /// </summary>
    public double DistanceKmTo(GeoLocation other)
    {
        var dLat = ToRadians(other.Latitude - Latitude);
        var dLon = ToRadians(other.Longitude - Longitude);
        var a = Math.Pow(Math.Sin(dLat / 2), 2) +
                Math.Cos(ToRadians(Latitude)) * Math.Cos(ToRadians(other.Latitude)) * Math.Pow(Math.Sin(dLon / 2), 2);
        return 2 * EarthRadiusKm * Math.Asin(Math.Sqrt(a));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
