using Shared.Domain.Primitives;

namespace PaymentTracking.Domain.ValueObjects;

/// <summary>
/// Represents monetary value with currency for payments.
/// </summary>
public sealed class PaymentAmount : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private PaymentAmount(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required", nameof(currency));
            
        var normalizedCurrency = currency.ToUpperInvariant().Trim();
        if (normalizedCurrency.Length != 3)
            throw new ArgumentException("Currency code must be exactly 3 characters (ISO 4217 format, e.g., USD, EUR, GBP)", nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public static PaymentAmount Create(decimal amount, string currency = "USD") => new(amount, currency);
    public static PaymentAmount Zero(string currency = "USD") => new(0, currency);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}
