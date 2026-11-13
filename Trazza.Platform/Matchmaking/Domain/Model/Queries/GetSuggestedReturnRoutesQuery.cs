using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Queries;

public record GetSuggestedReturnRoutesQuery(UserId UserId, ShipmentRequestId ShipmentRequestId);
