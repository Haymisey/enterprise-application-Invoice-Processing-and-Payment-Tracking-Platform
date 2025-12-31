using Shared.Domain.Primitives;

namespace VendorManagement.Domain.ValueObjects;

/// <summary>
/// Represents a unique identifier for a Vendor.
/// </summary>
public sealed class VendorId : ValueObject
{
    public Guid Value { get; }

    private VendorId(Guid value)
    {
        Value = value;
    }

    public static VendorId Create() => new(Guid.NewGuid());
    public static VendorId Create(Guid value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(VendorId id) => id.Value;
    public static explicit operator VendorId(Guid id) => Create(id);
}
