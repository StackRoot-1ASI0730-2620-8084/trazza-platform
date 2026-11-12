using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Billing.Domain.Model.ValueObjects;

public record Plan(string Code)
{
    public const string Free = "free";
    public const string Pro = "pro";

    private static readonly IReadOnlyDictionary<string, (decimal Price, int? MonthlyPublications)> Catalog =
        new Dictionary<string, (decimal Price, int? MonthlyPublications)>
        {
            [Free] = (0m, 5),
            [Pro] = (39.90m, null)
        };

    public Plan() : this(string.Empty)
    {
    }

    public static IReadOnlyList<Plan> All => Catalog.Keys.Select(code => new Plan(code)).ToList();

    public static Plan From(string? code) => new((code ?? string.Empty).Trim().ToLowerInvariant());

    public bool IsValid => Catalog.ContainsKey(Code);

    public bool IsFree => Code == Free;

    public Money Price => new(IsValid ? Catalog[Code].Price : 0m);

    public int? MonthlyPublications => IsValid ? Catalog[Code].MonthlyPublications : 0;

    public bool AllowsPublication(int usedThisMonth) =>
        MonthlyPublications is null || usedThisMonth < MonthlyPublications;
}
