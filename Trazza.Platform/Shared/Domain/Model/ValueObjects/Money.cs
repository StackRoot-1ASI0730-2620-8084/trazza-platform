using System;
using System.Globalization;

namespace Trazza.Platform.Shared.Domain.Model.ValueObjects;

/// <summary>
///     Immutable amount of money with its ISO 4217 currency code.
/// </summary>
public record Money
{
    public const string Soles = "PEN";
    public const string Dollars = "USD";
    public const string DefaultCurrency = Soles;

    public Money() : this(0m)
    {
    }

    public Money(decimal amount, string currency = DefaultCurrency)
    {
        Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = string.IsNullOrWhiteSpace(currency) ? DefaultCurrency : currency.Trim().ToUpperInvariant();
    }

    public decimal Amount { get; private set; }

    public string Currency { get; private set; }

    public bool IsValid => Amount >= 0 && Currency is Soles or Dollars;

    public string Formatted => Currency switch
    {
        Soles => $"S/ {Amount.ToString("N2", CultureInfo.InvariantCulture)}",
        Dollars => $"$ {Amount.ToString("N2", CultureInfo.InvariantCulture)}",
        _ => $"{Currency} {Amount.ToString("N2", CultureInfo.InvariantCulture)}"
    };

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor)
    {
        return new Money(Amount * factor, Currency);
    }

    public bool IsGreaterThan(Money other)
    {
        EnsureSameCurrency(other);
        return Amount > other.Amount;
    }

    private void EnsureSameCurrency(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"Cannot operate money in different currencies: {Currency} and {other.Currency}.");
    }
}