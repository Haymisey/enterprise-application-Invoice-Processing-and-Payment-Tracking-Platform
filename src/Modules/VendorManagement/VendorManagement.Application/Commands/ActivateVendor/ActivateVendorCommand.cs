using Shared.Application.Messaging;

namespace VendorManagement.Application.Commands.ActivateVendor;

/// <summary>
/// Command to activate a vendor.
/// </summary>
public sealed record ActivateVendorCommand(
    Guid VendorId,
    string ActivatedBy) : ICommand;
