using FluentValidation;

namespace InvoiceManagement.Application.Commands.CreateInvoice;

/// <summary>
/// Validator for CreateInvoiceCommand.
/// Uses FluentValidation for declarative validation rules.
/// </summary>
public sealed class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.InvoiceNumber)
            .NotEmpty().WithMessage("Invoice number is required")
            .MaximumLength(50).WithMessage("Invoice number cannot exceed 50 characters");

        RuleFor(x => x.VendorId)
            .NotEmpty().WithMessage("Vendor is required");

        RuleFor(x => x.IssueDate)
            .NotEmpty().WithMessage("Issue date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Issue date cannot be in the future");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Due date is required")
            .GreaterThanOrEqualTo(x => x.IssueDate).WithMessage("Due date must be after issue date");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("Created by is required");

        RuleFor(x => x.LineItems)
            .NotEmpty().WithMessage("At least one line item is required");

        RuleForEach(x => x.LineItems).ChildRules(lineItem =>
        {
            lineItem.RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Line item description is required");
            
            lineItem.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero");
            
            lineItem.RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative");
        });
    }
}
