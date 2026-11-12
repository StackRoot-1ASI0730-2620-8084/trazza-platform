using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Interfaces.Rest.Resources;

namespace Trazza.Platform.Matchmaking.Interfaces.Rest.Transform;

public static class MessageResourceFromEntityAssembler
{
    public static MessageResource ToResourceFromEntity(Message message)
    {
        return new MessageResource(
            message.Id,
            message.MatchProposalId,
            message.SenderUserId,
            message.Content,
            message.SentAt,
            message.ReadAt);
    }
}
