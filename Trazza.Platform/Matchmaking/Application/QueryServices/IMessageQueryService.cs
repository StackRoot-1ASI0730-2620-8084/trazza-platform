using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.Queries;
using Trazza.Platform.Shared.Application.Model;

namespace Trazza.Platform.Matchmaking.Application.QueryServices;

public interface IMessageQueryService
{
    /// <summary>
    ///     Fails when the user is not one of the two participants of the proposal.
    /// </summary>
    Task<Result<IEnumerable<Message>>> Handle(GetMessagesByProposalQuery query, CancellationToken cancellationToken);
}
