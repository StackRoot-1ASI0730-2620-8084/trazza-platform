using System.Globalization;

namespace Trazza.Platform.Shared.Domain.Model.ValueObjects;

/// <summary>
///     Geographic coordinates (WGS 84, SRID 4326) in decimal degrees.
/// </summary>
public record GeoLocation(double Latitude, double Longitude)
{
    private const double EarthRadiusKm = 6371;
    private const double KmPerDegree = 111.32;

    public static Result<GeoLocation, CommandError> Create(double latitude, double longitude)
    {
        if (!double.IsFinite(latitude) || latitude is < -90 or > 90)
            return CommandError.Validation("latitude-invalid");
        if (!double.IsFinite(longitude) || longitude is < -180 or > 180)
            return CommandError.Validation("longitude-invalid");
        return new GeoLocation(latitude, longitude);
    }

    /// <summary>
    ///     Well-known text in latitude-longitude order, used with <c>ST_PointFromText(..., 4326, 'axis-order=lat-long')</c>.
    /// </summary>
    public string ToWkt() => string.Create(CultureInfo.InvariantCulture, $"POINT({Latitude} {Longitude})");

    /// <summary>
    ///     Great-circle distance in kilometers (haversine formula).
    /// </summary>
    public double DistanceTo(GeoLocation other)
    {
        static double ToRadians(double degrees) => degrees * Math.PI / 180;
        var deltaLat = ToRadians(other.Latitude - Latitude);
        var deltaLng = ToRadians(other.Longitude - Longitude);
        var a = Math.Pow(Math.Sin(deltaLat / 2), 2)
                + Math.Cos(ToRadians(Latitude)) * Math.Cos(ToRadians(other.Latitude)) * Math.Pow(Math.Sin(deltaLng / 2), 2);
        return 2 * EarthRadiusKm * Math.Asin(Math.Sqrt(a));
    }

    /// <summary>
    ///     Approximate distance in kilometers from this location to the segment between
    ///     <paramref name="start" /> and <paramref name="end" />; used to detect route deviations.
    /// </summary>
    public double DistanceToSegment(GeoLocation start, GeoLocation end)
    {
        var cosLat = Math.Cos(Latitude * Math.PI / 180);
        (double X, double Y) Project(GeoLocation location) =>
            (location.Longitude * KmPerDegree * cosLat, location.Latitude * KmPerDegree);

        var p = Project(this);
        var a = Project(start);
        var b = Project(end);
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        var lengthSquared = dx * dx + dy * dy;
        var t = lengthSquared == 0 ? 0 : Math.Clamp(((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lengthSquared, 0, 1);
        return Math.Sqrt(Math.Pow(p.X - (a.X + t * dx), 2) + Math.Pow(p.Y - (a.Y + t * dy), 2));
    }
}
