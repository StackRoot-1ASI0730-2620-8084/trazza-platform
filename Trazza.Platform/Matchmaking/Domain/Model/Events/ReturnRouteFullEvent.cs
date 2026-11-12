using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Events;

namespace Trazza.Platform.Matchmaking.Domain.Model.Events;

/// <summary>
///     Raised when a return route reaches its maximum number of accepted shipments.
/// </summary>
public record ReturnRouteFullEvent(ReturnRouteId ReturnRouteId) : IEvent;
