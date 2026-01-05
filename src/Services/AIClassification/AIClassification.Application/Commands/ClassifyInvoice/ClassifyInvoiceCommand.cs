using Shared.Application.Messaging;

namespace AIClassification.Application.Commands.ClassifyInvoice;

public sealed record ClassifyInvoiceCommand(string ImageUrl) : ICommand<Guid>;