namespace VendorManagement.Domain.ValueObjects;

/// <summary>
/// Represents the status of a Vendor.
/// </summary>
public enum VendorStatus
{
    /// <summary>Vendor is pending approval.</summary>
    Pending = 0,

    /// <summary>Vendor is active and can receive payments.</summary>
    Active = 1,

    /// <summary>Vendor is temporarily suspended.</summary>
    Suspended = 2,

    /// <summary>Vendor is no longer active.</summary>
    Inactive = 3
}
