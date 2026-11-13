namespace Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

public record TimeWindow(DateTimeOffset From, DateTimeOffset Until)
{
    public TimeWindow() : this(default, default)
    {
    }

    public bool IsValid => Until > From;

    public bool Contains(DateTimeOffset moment) => moment >= From && moment <= Until;
}
