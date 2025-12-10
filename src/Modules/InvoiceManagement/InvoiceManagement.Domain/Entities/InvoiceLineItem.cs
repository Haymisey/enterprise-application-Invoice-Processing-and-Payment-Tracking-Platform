using Shared.Domain.Primitives;
using InvoiceManagement.Domain.ValueObjects;

namespace InvoiceManagement.Domain.Entities;

/// <summary>
/// Represents an individual line item on an invoice.
/// Entity within the Invoice aggregate - identified by ID but owned by Invoice.
/// </summary>
public sealed class InvoiceLineItem : Entity<LineItemId>
{
    public string Description { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public Money TotalPrice => UnitPrice.Multiply(Quantity);

    private InvoiceLineItem(
        LineItemId id,
        string description,
        int quantity,
        Money unitPrice) : base(id)
    {
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    // Required for EF Core
    private InvoiceLineItem() : base() 
    {
        Description = string.Empty;
        UnitPrice = Money.Zero();
    }

    public static InvoiceLineItem Create(
        string description,
        int quantity,
        decimal unitPrice,
        string currency = "USD")
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        return new InvoiceLineItem(
            LineItemId.Create(),
            description,
            quantity,
            Money.Create(unitPrice, currency));
    }

    public void Update(string description, int quantity, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        Description = description;
        Quantity = quantity;
        UnitPrice = Money.Create(unitPrice, UnitPrice.Currency);
    }
}
