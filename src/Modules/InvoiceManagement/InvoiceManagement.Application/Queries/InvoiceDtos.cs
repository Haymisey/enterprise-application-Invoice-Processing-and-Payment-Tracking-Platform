using InvoiceManagement.Domain.ValueObjects;

namespace InvoiceManagement.Application.Queries;

/// <summary>
/// DTO for Invoice data returned by queries.
/// </summary>
public sealed record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    Guid VendorId,
    string VendorName,
    string Status,
    DateTime IssueDate,
    DateTime DueDate,
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount,
    string Currency,
    string? Notes,
    string CreatedBy,
    DateTime CreatedAt,
    string? ApprovedBy,
    DateTime? ApprovedAt,
    List<InvoiceLineItemDto> LineItems);

public sealed record InvoiceLineItemDto(
    Guid Id,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);

/// <summary>
/// Summary DTO for invoice list views.
/// </summary>
public sealed record InvoiceSummaryDto(
    Guid Id,
    string InvoiceNumber,
    string VendorName,
    string Status,
    DateTime DueDate,
    decimal TotalAmount,
    string Currency,
    bool IsOverdue);
