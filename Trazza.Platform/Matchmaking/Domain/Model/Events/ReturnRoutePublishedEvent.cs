using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Events;

namespace Trazza.Platform.Matchmaking.Domain.Model.Events;

/// <summary>
///     Raised when a carrier publishes a return route.
/// </summary>
public record ReturnRoutePublishedEvent(ReturnRouteId ReturnRouteId, CarrierProfileId CarrierProfileId,
    DateTimeOffset DepartureAt) : IEvent;
