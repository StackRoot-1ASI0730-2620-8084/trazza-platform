using Cortex.Mediator.Notifications;

namespace Trazza.Platform.Shared.Domain.Model.Events;

/// <summary>
///     Marker interface for domain events. Extends the Cortex.Mediator notification
///     so events can be published to their handlers after a successful commit.
/// </summary>
public interface IEvent : INotification;