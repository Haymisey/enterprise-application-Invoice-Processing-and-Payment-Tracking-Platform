using Shared.Domain.Primitives;

namespace AIClassification.Domain.ValueObjects;

public sealed class ClassificationId : ValueObject
{
    public Guid Value { get; }

    private ClassificationId(Guid value) => Value = value;

    public static ClassificationId Create() => new(Guid.NewGuid());
    public static ClassificationId Create(Guid value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator Guid(ClassificationId id) => id.Value;
}