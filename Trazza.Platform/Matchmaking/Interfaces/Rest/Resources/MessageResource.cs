namespace Trazza.Platform.Matchmaking.Interfaces.Rest.Resources;

public record MessageResource(
    ulong Id,
    Guid MatchProposalId,
    ulong SenderUserId,
    string Content,
    DateTimeOffset SentAt,
    DateTimeOffset? ReadAt);
