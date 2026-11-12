using Trazza.Platform.Matchmaking.Application.Internal.OutboundServices;
using Trazza.Platform.Matchmaking.Application.QueryServices;
using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.Errors;
using Trazza.Platform.Matchmaking.Domain.Model.Queries;
using Trazza.Platform.Matchmaking.Domain.Repositories;
using Trazza.Platform.Shared.Application.Model;

namespace Trazza.Platform.Matchmaking.Application.Internal.QueryServices;

public class MessageQueryService(
    IMessageRepository messageRepository,
    IMatchProposalRepository matchProposalRepository,
    IReturnRouteRepository returnRouteRepository,
    IShipmentRequestRepository shipmentRequestRepository,
    IProfilesService profilesService)
    : IMessageQueryService
{
    public async Task<Result<IEnumerable<Message>>> Handle(GetMessagesByProposalQuery query,
        CancellationToken cancellationToken)
    {
        var proposal = await matchProposalRepository.FindByIdAsNoTrackingAsync(query.MatchProposalId,
            cancellationToken);
        if (proposal is null)
            return Result<IEnumerable<Message>>.Failure(MatchmakingErrors.MatchProposalNotFound);

        var route = await returnRouteRepository.FindByIdAsNoTrackingAsync(proposal.ReturnRouteId, cancellationToken);
        var shipment =
            await shipmentRequestRepository.FindByIdAsNoTrackingAsync(proposal.ShipmentRequestId, cancellationToken);
        if (route is null || shipment is null)
            return Result<IEnumerable<Message>>.Failure(MatchmakingErrors.MatchProposalNotFound);

        var carrierProfileId = await profilesService.FetchCarrierProfileIdByUserId(query.UserId, cancellationToken);
        var merchantProfileId = await profilesService.FetchMerchantProfileIdByUserId(query.UserId, cancellationToken);

        var isParticipant = carrierProfileId == route.CarrierProfileId ||
                            merchantProfileId == shipment.MerchantProfileId;
        if (!isParticipant)
            return Result<IEnumerable<Message>>.Failure(MatchmakingErrors.NotProposalParticipant);

        var messages =
            await messageRepository.FindByMatchProposalIdAsNoTrackingAsync(query.MatchProposalId, cancellationToken);
        return Result<IEnumerable<Message>>.Success(messages);
    }
}
