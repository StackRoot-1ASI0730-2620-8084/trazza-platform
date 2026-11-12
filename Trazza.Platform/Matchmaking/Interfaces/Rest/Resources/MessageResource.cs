namespace Trazza.Platform.Matchmaking.Interfaces.Rest.Resources;

public record MessageResource(
    ulong Id,
    ulong MatchProposalId,
    ulong SenderUserId,
    string Content,
    DateTimeOffset SentAt,
    DateTimeOffset? ReadAt);
