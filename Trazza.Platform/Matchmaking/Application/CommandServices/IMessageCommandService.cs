using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.Commands;
using Trazza.Platform.Shared.Application.Model;

namespace Trazza.Platform.Matchmaking.Application.CommandServices;

public interface IMessageCommandService
{
    Task<Result<Message>> Handle(SendMessageCommand command, CancellationToken cancellationToken);

    /// <summary>
    ///     Marks as read the messages sent by the other participant. Returns how many were marked.
    /// </summary>
    Task<Result<int>> Handle(MarkMessagesAsReadCommand command, CancellationToken cancellationToken);
}
