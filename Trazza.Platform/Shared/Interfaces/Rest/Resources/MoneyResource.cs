namespace Trazza.Platform.Shared.Interfaces.Rest.Resources;

/// <summary>
///     REST representation of an amount of money. Serialized as { "amount": 10.00, "currency": "USD" },
///     the same shape the Money value object of trazza-webapp expects.
/// </summary>
/// <param name="Amount">Amount rounded to two decimals.</param>
/// <param name="Currency">ISO 4217 currency code (PEN or USD).</param>
public record MoneyResource(decimal Amount, string Currency);
