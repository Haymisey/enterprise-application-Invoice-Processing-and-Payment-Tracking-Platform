using Shared.Domain.Primitives;
using VendorManagement.Domain.Events;
using VendorManagement.Domain.ValueObjects;

namespace VendorManagement.Domain.Aggregates;

/// <summary>
/// Vendor Aggregate Root.
/// Manages vendor profiles and their lifecycle.
/// </summary>
public sealed class Vendor : AggregateRoot<VendorId>
{
    public string Name { get; private set; }
    public string TaxId { get; private set; }
    public VendorStatus Status { get; private set; }
    public ContactInfo Contact { get; private set; }
    public Address? Address { get; private set; }
    public string? BankAccountNumber { get; private set; }
    public string? BankRoutingNumber { get; private set; }
    public int PaymentTermDays { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; }
    public DateTime? ActivatedAt { get; private set; }
    public string? ActivatedBy { get; private set; }

    private Vendor(
        VendorId id,
        string name,
        string taxId,
        ContactInfo contact,
        string createdBy,
        int paymentTermDays = 30) : base(id)
    {
        Name = name;
        TaxId = taxId;
        Contact = contact;
        Status = VendorStatus.Pending;
        PaymentTermDays = paymentTermDays;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
    }

    // Required for EF Core
    private Vendor() : base()
    {
        Name = string.Empty;
        TaxId = string.Empty;
        Contact = ContactInfo.Create("placeholder@example.com", "");
        CreatedBy = string.Empty;
    }

    /// <summary>
    /// Factory method to register a new vendor.
    /// </summary>
    public static Vendor Register(
        string name,
        string taxId,
        string email,
        string phone,
        string? contactPerson,
        string createdBy,
        int paymentTermDays = 30)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Vendor name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(taxId))
            throw new ArgumentException("Tax ID is required", nameof(taxId));

        var vendor = new Vendor(
            VendorId.Create(),
            name,
            taxId,
            ContactInfo.Create(email, phone, contactPerson),
            createdBy,
            paymentTermDays);

        vendor.RaiseDomainEvent(new VendorRegisteredEvent(
            vendor.Id.Value,
            name,
            email));

        return vendor;
    }

    /// <summary>
    /// Activate the vendor (approve for payments).
    /// </summary>
    public void Activate(string activatedBy)
    {
        if (string.IsNullOrWhiteSpace(activatedBy))
            throw new ArgumentException("Activated by is required", nameof(activatedBy));

        if (Status == VendorStatus.Active)
            throw new InvalidOperationException("Vendor is already active");

        Status = VendorStatus.Active;
        ActivatedAt = DateTime.UtcNow;
        ActivatedBy = activatedBy;

        RaiseDomainEvent(new VendorActivatedEvent(Id.Value, activatedBy));
    }

    /// <summary>
    /// Suspend the vendor.
    /// </summary>
    public void Suspend(string reason, string suspendedBy)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Suspension reason is required", nameof(reason));

        if (Status != VendorStatus.Active)
            throw new InvalidOperationException($"Cannot suspend vendor in {Status} status");

        Status = VendorStatus.Suspended;
        Notes = $"{Notes}\n[SUSPENDED]: {reason} by {suspendedBy} on {DateTime.UtcNow:g}";

        RaiseDomainEvent(new VendorSuspendedEvent(Id.Value, reason, suspendedBy));
    }

    /// <summary>
    /// Deactivate the vendor.
    /// </summary>
    public void Deactivate()
    {
        Status = VendorStatus.Inactive;
    }

    /// <summary>
    /// Update vendor details.
    /// </summary>
    public void UpdateDetails(
        string name,
        string email,
        string phone,
        string? contactPerson,
        int paymentTermDays)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        Name = name;
        Contact = ContactInfo.Create(email, phone, contactPerson);
        PaymentTermDays = paymentTermDays;

        RaiseDomainEvent(new VendorUpdatedEvent(Id.Value, name));
    }

    /// <summary>
    /// Update vendor address.
    /// </summary>
    public void UpdateAddress(string street, string city, string state, string postalCode, string country)
    {
        Address = ValueObjects.Address.Create(street, city, state, postalCode, country);
    }

    /// <summary>
    /// Update bank details.
    /// </summary>
    public void UpdateBankDetails(string accountNumber, string routingNumber)
    {
        BankAccountNumber = accountNumber;
        BankRoutingNumber = routingNumber;
    }
}
