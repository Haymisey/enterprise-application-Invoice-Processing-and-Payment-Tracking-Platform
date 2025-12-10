using Shared.Domain.Primitives;

namespace PaymentTracking.Domain.ValueObjects;

/// <summary>
/// Represents a unique identifier for a Payment.
/// </summary>
public sealed class PaymentId : ValueObject
{
    public Guid Value { get; }

    private PaymentId(Guid value)
    {
        Value = value;
    }

    public static PaymentId Create() => new(Guid.NewGuid());
    public static PaymentId Create(Guid value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(PaymentId id) => id.Value;
    public static explicit operator PaymentId(Guid id) => Create(id);
}
