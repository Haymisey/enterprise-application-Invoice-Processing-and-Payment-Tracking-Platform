using Shared.Domain.Primitives;

namespace InvoiceManagement.Domain.ValueObjects;

/// <summary>
/// Represents a unique identifier for an Invoice.
/// Strongly-typed ID to prevent primitive obsession.
/// </summary>
public sealed class InvoiceId : ValueObject
{
    public Guid Value { get; }

    private InvoiceId(Guid value)
    {
        Value = value;
    }

    public static InvoiceId Create() => new(Guid.NewGuid());
    public static InvoiceId Create(Guid value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(InvoiceId id) => id.Value;
    public static explicit operator InvoiceId(Guid id) => Create(id);
}
