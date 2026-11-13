using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Interfaces.Rest.Resources;

namespace Trazza.Platform.Matchmaking.Interfaces.Rest.Transform;

public static class MessageResourceFromEntityAssembler
{
    public static MessageResource ToResourceFromEntity(Message message, Guid matchProposalPublicId)
    {
        return new MessageResource(
            message.Id,
            matchProposalPublicId,
            message.SenderUserId,
            message.Content,
            message.SentAt,
            message.ReadAt);
    }
}
