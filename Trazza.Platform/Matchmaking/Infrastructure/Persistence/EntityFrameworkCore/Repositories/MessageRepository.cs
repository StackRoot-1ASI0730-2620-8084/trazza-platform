using Microsoft.EntityFrameworkCore;
using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Matchmaking.Domain.Repositories;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

namespace Trazza.Platform.Matchmaking.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class MessageRepository(AppDbContext context) : BaseRepository<Message>(context), IMessageRepository
{
    public async Task<IEnumerable<Message>> FindByMatchProposalIdAsNoTrackingAsync(MatchProposalId matchProposalId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<Message>()
            .AsNoTracking()
            .Where(message => message.MatchProposalId == matchProposalId)
            .OrderBy(message => message.SentAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> DeleteByMatchProposalIdAsync(MatchProposalId matchProposalId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<Message>()
            .Where(message => message.MatchProposalId == matchProposalId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<IEnumerable<Message>> ListUnreadSentByOthersAsync(MatchProposalId matchProposalId,
        UserId reader, CancellationToken cancellationToken)
    {
        return await Context.Set<Message>()
            .Where(message => message.MatchProposalId == matchProposalId &&
                              message.SenderUserId != reader &&
                              message.ReadAt == null)
            .ToListAsync(cancellationToken);
    }
}
