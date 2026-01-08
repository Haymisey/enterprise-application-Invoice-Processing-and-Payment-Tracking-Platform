using Shared.Application.Messaging;

namespace InvoiceManagement.Application.Commands.FlagInvoice;

/// <summary>
/// Command to flag an invoice for manual review.
/// </summary>
public sealed record FlagInvoiceCommand(
    Guid InvoiceId,
    string Reason,
    string FlaggedBy) : ICommand;
