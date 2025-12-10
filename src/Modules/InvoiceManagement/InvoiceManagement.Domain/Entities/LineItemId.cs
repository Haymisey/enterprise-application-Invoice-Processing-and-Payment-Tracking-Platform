using Shared.Domain.Primitives;

namespace InvoiceManagement.Domain.Entities;

/// <summary>
/// Represents a unique identifier for an Invoice Line Item.
/// </summary>
public sealed class LineItemId : ValueObject
{
    public Guid Value { get; }

    private LineItemId(Guid value)
    {
        Value = value;
    }

    public static LineItemId Create() => new(Guid.NewGuid());
    public static LineItemId Create(Guid value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator Guid(LineItemId id) => id.Value;
}
