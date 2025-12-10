using Shared.Application.Messaging;

namespace InvoiceManagement.Application.Commands.ApproveInvoice;

/// <summary>
/// Command to approve an invoice for payment.
/// </summary>
public sealed record ApproveInvoiceCommand(
    Guid InvoiceId,
    string ApprovedBy) : ICommand;
