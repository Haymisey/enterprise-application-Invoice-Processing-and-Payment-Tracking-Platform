using Shared.Application.Messaging;

namespace InvoiceManagement.Application.Commands.CreateInvoice;

/// <summary>
/// Command to create a new invoice.
/// </summary>
public sealed record CreateInvoiceCommand(
    string InvoiceNumber,
    Guid VendorId,
    DateTime IssueDate,
    DateTime DueDate,
    string CreatedBy,
    string? Notes,
    List<CreateInvoiceLineItemDto> LineItems,
    Guid? ClassificationId = null) : ICommand<Guid>;

public sealed record CreateInvoiceLineItemDto(
    string Description,
    int Quantity,
    decimal UnitPrice);
