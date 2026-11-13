using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Events;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Events;

public record ProposalAcceptedEvent(MatchProposalId MatchProposalId, ShipmentRequestId ShipmentRequestId,
    ReturnRouteId ReturnRouteId, Money OfferedRate) : IEvent;
