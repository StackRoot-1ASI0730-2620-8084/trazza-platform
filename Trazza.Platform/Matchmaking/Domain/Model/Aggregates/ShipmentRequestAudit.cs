using Trazza.Platform.Shared.Domain.Model.Entities;

namespace Trazza.Platform.Matchmaking.Domain.Model.Aggregates;

public partial class ShipmentRequest : IAuditableEntity
{
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
