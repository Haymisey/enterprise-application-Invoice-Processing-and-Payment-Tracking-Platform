using InvoiceManagement.Domain.Aggregates;
using InvoiceManagement.Domain.Entities;
using InvoiceManagement.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Invoice aggregate.
/// Maps domain entities to database tables while preserving DDD patterns.
/// </summary>
public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(i => i.Id);

        // Configure InvoiceId as a value object
        builder.Property(i => i.Id)
            .HasConversion(
                id => id.Value,
                value => InvoiceId.Create(value))
            .HasColumnName("Id");

        builder.Property(i => i.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(i => i.InvoiceNumber)
            .IsUnique();

        builder.Property(i => i.VendorId)
            .HasConversion(
                id => id.Value,
                value => VendorId.Create(value))
            .HasColumnName("VendorId");

        builder.Property(i => i.ClassificationId)
            .IsRequired(false);

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Configure InvoiceDates value object (owned type)
        builder.OwnsOne(i => i.Dates, dates =>
        {
            dates.Property(d => d.IssueDate).HasColumnName("IssueDate").IsRequired();
            dates.Property(d => d.DueDate).HasColumnName("DueDate").IsRequired();
        });

        // Configure Money value objects (owned types)
        builder.OwnsOne(i => i.SubTotal, money =>
        {
            money.Property(m => m.Amount).HasColumnName("SubTotal").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("SubTotalCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(i => i.TaxAmount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("TaxAmount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("TaxAmountCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(i => i.TotalAmount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("TotalAmount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("TotalAmountCurrency").HasMaxLength(3);
        });

        builder.Property(i => i.Notes).HasMaxLength(1000);
        builder.Property(i => i.RejectionReason).HasMaxLength(500);
        builder.Property(i => i.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(i => i.ApprovedBy).HasMaxLength(100);

        // Configure relationship with line items
        builder.HasMany(i => i.LineItems)
            .WithOne()
            .HasForeignKey("InvoiceId")
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events (not persisted)
        builder.Ignore(i => i.DomainEvents);
    }
}

/// <summary>
/// EF Core configuration for InvoiceLineItem entity.
/// </summary>
public sealed class InvoiceLineItemConfiguration : IEntityTypeConfiguration<InvoiceLineItem>
{
    public void Configure(EntityTypeBuilder<InvoiceLineItem> builder)
    {
        builder.ToTable("InvoiceLineItems");

        builder.HasKey(li => li.Id);

        builder.Property(li => li.Id)
            .HasConversion(
                id => id.Value,
                value => LineItemId.Create(value))
            .HasColumnName("Id");

        builder.Property(li => li.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(li => li.Quantity)
            .IsRequired();

        // Configure UnitPrice Money value object
        builder.OwnsOne(li => li.UnitPrice, money =>
        {
            money.Property(m => m.Amount).HasColumnName("UnitPrice").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
        });

        // TotalPrice is calculated, not stored
        builder.Ignore(li => li.TotalPrice);
    }
}
