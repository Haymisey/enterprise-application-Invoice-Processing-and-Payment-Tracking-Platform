using Shared.Domain.Primitives;

namespace VendorManagement.Domain.ValueObjects;

/// <summary>
/// Represents a vendor's contact information.
/// </summary>
public sealed class ContactInfo : ValueObject
{
    public string Email { get; }
    public string Phone { get; }
    public string? ContactPerson { get; }

    private ContactInfo(string email, string phone, string? contactPerson)
    {
        Email = email;
        Phone = phone;
        ContactPerson = contactPerson;
    }

    public static ContactInfo Create(string email, string phone, string? contactPerson = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));
        if (!email.Contains('@'))
            throw new ArgumentException("Invalid email format", nameof(email));

        return new ContactInfo(email, phone ?? string.Empty, contactPerson);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Email;
        yield return Phone;
        yield return ContactPerson ?? string.Empty;
    }

    public override string ToString() => ContactPerson is not null 
        ? $"{ContactPerson} ({Email})" 
        : Email;
}
