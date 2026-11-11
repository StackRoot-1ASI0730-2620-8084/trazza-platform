using System;

namespace Trazza.Platform.Shared.Domain.Model.Entities;

/// <summary>
///     Contract for entities that keep their creation and last update timestamps.
///     The values are set by the AuditableEntityInterceptor when changes are saved.
/// </summary>
public interface IAuditableEntity
{
    DateTimeOffset? CreatedAt { get; set; }

    DateTimeOffset? UpdatedAt { get; set; }
}