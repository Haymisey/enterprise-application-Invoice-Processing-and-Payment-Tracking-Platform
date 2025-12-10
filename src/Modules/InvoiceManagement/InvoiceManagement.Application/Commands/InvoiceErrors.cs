using Shared.Domain.Results;

namespace InvoiceManagement.Application.Commands;

/// <summary>
/// Domain-specific errors for Invoice operations.
/// </summary>
public static class InvoiceErrors
{
    public static Error NotFound(Guid invoiceId) => 
        new("Invoice.NotFound", $"Invoice with ID '{invoiceId}' was not found.");

    public static Error DuplicateInvoiceNumber(string invoiceNumber) => 
        new("Invoice.DuplicateNumber", $"Invoice with number '{invoiceNumber}' already exists.");

    public static Error InvalidStatusTransition(string currentStatus, string action) => 
        new("Invoice.InvalidStatus", $"Cannot {action} invoice in {currentStatus} status.");

    public static Error NoLineItems => 
        new("Invoice.NoLineItems", "Invoice must have at least one line item.");

    public static Error AlreadyApproved => 
        new("Invoice.AlreadyApproved", "Invoice has already been approved.");

    public static Error AlreadyPaid => 
        new("Invoice.AlreadyPaid", "Invoice has already been paid.");
}
