using Trazza.Platform.Matchmaking.Domain.Model.Errors;
using Trazza.Platform.Matchmaking.Domain.Model.Events;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Aggregates.Internal;

namespace Trazza.Platform.Matchmaking.Domain.Model.Aggregates;

public class Message : AggregateRoot
{
    public const int MaxContentLength = 2000;

    public Message()
    {
        Content = string.Empty;
    }

    public Message(MatchProposalId matchProposalId, UserId senderUserId, string content, DateTimeOffset sentAt) : this()
    {
        MatchProposalId = matchProposalId;
        SenderUserId = senderUserId;
        Content = content.Trim();
        SentAt = sentAt;
    }

    public MessageId Id { get; }
    public MatchProposalId MatchProposalId { get; private set; }
    public UserId SenderUserId { get; private set; }
    public string Content { get; private set; }
    public DateTimeOffset SentAt { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }

    public bool IsRead => ReadAt is not null;

    public void RaiseSent() => AddDomainEvent(new MessageSentEvent(MatchProposalId, SenderUserId));

    public void MarkAsRead(DateTimeOffset now) => ReadAt ??= now;

    public static bool IsValidContent(string? content) =>
        !string.IsNullOrWhiteSpace(content) && content.Trim().Length <= MaxContentLength;
}
