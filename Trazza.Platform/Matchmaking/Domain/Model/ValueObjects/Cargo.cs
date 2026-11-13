namespace Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

public record Cargo
{
    public Cargo() : this(0m, 0m)
    {
    }

    public Cargo(decimal weightKg, decimal volumeM3)
    {
        WeightKg = Math.Round(weightKg, 2, MidpointRounding.AwayFromZero);
        VolumeM3 = Math.Round(volumeM3, 2, MidpointRounding.AwayFromZero);
    }

    public decimal WeightKg { get; private set; }

    public decimal VolumeM3 { get; private set; }

    public bool IsValid => WeightKg > 0 && VolumeM3 > 0;
}
