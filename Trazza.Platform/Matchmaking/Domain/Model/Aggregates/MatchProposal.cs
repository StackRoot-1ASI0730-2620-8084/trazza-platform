using Trazza.Platform.Matchmaking.Domain.Model.Errors;
using Trazza.Platform.Matchmaking.Domain.Model.Events;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Aggregates.Internal;
using Trazza.Platform.Shared.Domain.Model.Errors;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Aggregates;

/// <summary>
///     The single offer a carrier makes for a shipment request using one of its routes (table match_proposals).
///     There are no counteroffers: the merchant accepts or rejects.
/// </summary>
public partial class MatchProposal : AggregateRoot
{
    public MatchProposal()
    {
        OfferedRate = new Money();
    }

    public MatchProposal(ShipmentRequestId shipmentRequestId, ReturnRouteId returnRouteId, Money offeredRate) : this()
    {
        ShipmentRequestId = shipmentRequestId;
        ReturnRouteId = returnRouteId;
        OfferedRate = offeredRate;
        Status = EMatchProposalStatus.Pending;
    }

    public MatchProposalId Id { get; }
    public ShipmentRequestId ShipmentRequestId { get; private set; }
    public ReturnRouteId ReturnRouteId { get; private set; }
    public Money OfferedRate { get; private set; }
    public EMatchProposalStatus Status { get; private set; }
    public DateTimeOffset? RespondedAt { get; private set; }

    public bool IsPending => Status == EMatchProposalStatus.Pending;

    public bool IsAccepted => Status == EMatchProposalStatus.Accepted;

    /// <summary>
    ///     Raises the submitted event. Call it after the first save so the generated id is already assigned.
    /// </summary>
    public void RaiseSubmitted() => AddDomainEvent(new MatchProposalSubmittedEvent(Id, ShipmentRequestId, ReturnRouteId));

    public Error Accept(DateTimeOffset now)
    {
        if (!IsPending) return MatchmakingErrors.ProposalNotPending;

        Status = EMatchProposalStatus.Accepted;
        RespondedAt = now;
        AddDomainEvent(new ProposalAcceptedEvent(Id, ShipmentRequestId, ReturnRouteId, OfferedRate));
        return Error.None;
    }

    public Error Reject(DateTimeOffset now)
    {
        if (!IsPending) return MatchmakingErrors.ProposalNotPending;

        Status = EMatchProposalStatus.Rejected;
        RespondedAt = now;
        AddDomainEvent(new ProposalRejectedEvent(Id, ShipmentRequestId));
        return Error.None;
    }

    /// <summary>
    ///     Expires a pending proposal, for example when another proposal for the same shipment is accepted.
    /// </summary>
    public Error Expire()
    {
        if (!IsPending) return MatchmakingErrors.ProposalNotPending;

        Status = EMatchProposalStatus.Expired;
        return Error.None;
    }

    /// <summary>
    ///     The carrier withdraws a pending or accepted proposal.
    /// </summary>
    public Error Cancel(DateTimeOffset now)
    {
        if (Status is not (EMatchProposalStatus.Pending or EMatchProposalStatus.Accepted))
            return MatchmakingErrors.ProposalNotPending;

        Status = EMatchProposalStatus.Cancelled;
        RespondedAt ??= now;
        return Error.None;
    }
}
