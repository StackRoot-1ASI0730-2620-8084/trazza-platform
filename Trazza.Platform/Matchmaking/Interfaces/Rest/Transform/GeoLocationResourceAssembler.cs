using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Matchmaking.Interfaces.Rest.Resources;

namespace Trazza.Platform.Matchmaking.Interfaces.Rest.Transform;

public static class GeoLocationResourceAssembler
{
    public static GeoLocationResource ToResourceFromValueObject(GeoLocation location)
    {
        return new GeoLocationResource(location.Latitude, location.Longitude);
    }

    public static GeoLocation ToValueObjectFromResource(GeoLocationResource resource)
    {
        return new GeoLocation(resource.Latitude, resource.Longitude);
    }
}
