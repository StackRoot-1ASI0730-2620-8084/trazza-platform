using Trazza.Platform.Matchmaking.Domain.Model.Commands;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Matchmaking.Interfaces.Rest.Resources;

namespace Trazza.Platform.Matchmaking.Interfaces.Rest.Transform;

public static class SendMessageCommandFromResourceAssembler
{
    public static SendMessageCommand ToCommandFromResource(UserId userId, MatchProposalId matchProposalId,
        SendMessageResource resource)
    {
        return new SendMessageCommand(userId, matchProposalId, resource.Content);
    }
}
