namespace VendorManagement.Application.Queries;

/// <summary>
/// DTO for Vendor data returned by queries.
/// </summary>
public sealed record VendorDto(
    Guid Id,
    string Name,
    string TaxId,
    string Status,
    string Email,
    string Phone,
    string? ContactPerson,
    string? Street,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    int PaymentTermDays,
    string? Notes,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? ActivatedAt,
    string? ActivatedBy);

/// <summary>
/// Summary DTO for vendor list views.
/// </summary>
public sealed record VendorSummaryDto(
    Guid Id,
    string Name,
    string TaxId,
    string Status,
    string Email,
    int PaymentTermDays);
