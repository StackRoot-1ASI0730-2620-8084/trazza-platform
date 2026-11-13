using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Queries;

public record GetSuggestedShipmentRequestsQuery(UserId UserId, ReturnRouteId ReturnRouteId);
