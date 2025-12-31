using Shared.Domain.Results;

namespace VendorManagement.Application.Commands;

/// <summary>
/// Domain-specific errors for Vendor operations.
/// </summary>
public static class VendorErrors
{
    public static Error NotFound(Guid vendorId) => 
        new("Vendor.NotFound", $"Vendor with ID '{vendorId}' was not found.");

    public static Error DuplicateTaxId(string taxId) => 
        new("Vendor.DuplicateTaxId", $"Vendor with tax ID '{taxId}' already exists.");

    public static Error AlreadyActive => 
        new("Vendor.AlreadyActive", "Vendor is already active.");

    public static Error NotActive => 
        new("Vendor.NotActive", "Vendor is not active.");

    public static Error InvalidStatus(string currentStatus, string action) => 
        new("Vendor.InvalidStatus", $"Cannot {action} vendor in {currentStatus} status.");
}
