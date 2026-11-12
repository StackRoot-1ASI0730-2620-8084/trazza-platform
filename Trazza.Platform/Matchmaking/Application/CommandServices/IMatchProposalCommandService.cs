using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.Commands;
using Trazza.Platform.Shared.Application.Model;

namespace Trazza.Platform.Matchmaking.Application.CommandServices;

public interface IMatchProposalCommandService
{
    Task<Result<MatchProposal>> Handle(SubmitMatchProposalCommand command, CancellationToken cancellationToken);

    Task<Result<MatchProposal>> Handle(AcceptMatchProposalCommand command, CancellationToken cancellationToken);

    Task<Result<MatchProposal>> Handle(RejectMatchProposalCommand command, CancellationToken cancellationToken);

    Task<Result<MatchProposal>> Handle(CancelMatchProposalCommand command, CancellationToken cancellationToken);
}
