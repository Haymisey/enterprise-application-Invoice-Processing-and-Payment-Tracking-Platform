using Shared.Domain.Primitives;

namespace VendorManagement.Domain.ValueObjects;

/// <summary>
/// Represents a vendor's address.
/// </summary>
public sealed class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }
    public string Country { get; }

    private Address(string street, string city, string state, string postalCode, string country)
    {
        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    public static Address Create(string street, string city, string state, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required", nameof(city));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country is required", nameof(country));

        return new Address(
            street ?? string.Empty,
            city,
            state ?? string.Empty,
            postalCode ?? string.Empty,
            country);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }

    public override string ToString() => $"{Street}, {City}, {State} {PostalCode}, {Country}";
}
