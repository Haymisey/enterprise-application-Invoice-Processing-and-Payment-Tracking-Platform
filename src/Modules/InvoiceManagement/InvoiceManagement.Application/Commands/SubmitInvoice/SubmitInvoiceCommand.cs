using Shared.Application.Messaging;

namespace InvoiceManagement.Application.Commands.SubmitInvoice;

/// <summary>
/// Command to submit a draft invoice for approval.
/// </summary>
public sealed record SubmitInvoiceCommand(Guid InvoiceId) : ICommand;
