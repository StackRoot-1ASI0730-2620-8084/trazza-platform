using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.Queries;
using Trazza.Platform.Shared.Application.Model;

namespace Trazza.Platform.Matchmaking.Application.QueryServices;

public interface IMessageQueryService
{
    Task<Result<IEnumerable<Message>>> Handle(GetMessagesByProposalQuery query, CancellationToken cancellationToken);
}
