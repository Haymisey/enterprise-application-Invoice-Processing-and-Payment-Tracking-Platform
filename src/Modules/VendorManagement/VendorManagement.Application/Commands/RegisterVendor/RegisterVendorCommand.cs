using Shared.Application.Messaging;

namespace VendorManagement.Application.Commands.RegisterVendor;

/// <summary>
/// Command to register a new vendor.
/// </summary>
public sealed record RegisterVendorCommand(
    string Name,
    string TaxId,
    string Email,
    string Phone,
    string? ContactPerson,
    int PaymentTermDays,
    string CreatedBy) : ICommand<Guid>;
