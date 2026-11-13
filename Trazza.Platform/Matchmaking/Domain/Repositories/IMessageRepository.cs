using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Matchmaking.Domain.Repositories;

public interface IMessageRepository : IBaseRepository<Message>
{
    Task<IEnumerable<Message>> FindByMatchProposalIdAsNoTrackingAsync(MatchProposalId matchProposalId,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Unread messages of a proposal that were sent by someone else than the reader, tracked.
    /// </summary>
    /// <summary>
    ///     Deletes the chat of a proposal and returns how many messages were deleted.
    /// </summary>
    Task<int> DeleteByMatchProposalIdAsync(MatchProposalId matchProposalId, CancellationToken cancellationToken);

    Task<IEnumerable<Message>> ListUnreadSentByOthersAsync(MatchProposalId matchProposalId, UserId reader,
        CancellationToken cancellationToken);
}
