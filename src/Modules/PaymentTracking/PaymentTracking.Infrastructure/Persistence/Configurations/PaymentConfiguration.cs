using PaymentTracking.Domain.Aggregates;
using PaymentTracking.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaymentTracking.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Payment aggregate.
/// </summary>
public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(
                id => id.Value,
                value => PaymentId.Create(value))
            .HasColumnName("Id");

        builder.Property(p => p.InvoiceId).IsRequired();
        builder.HasIndex(p => p.InvoiceId).IsUnique();

        builder.Property(p => p.VendorId).IsRequired();

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Configure PaymentAmount value object
        builder.OwnsOne(p => p.Amount, amount =>
        {
            amount.Property(a => a.Amount).HasColumnName("Amount").HasPrecision(18, 2);
            amount.Property(a => a.Currency).HasColumnName("Currency").HasMaxLength(3);
        });

        builder.Property(p => p.ScheduledDate).IsRequired();
        builder.Property(p => p.ProcessedDate);
        builder.Property(p => p.CompletedDate);
        builder.Property(p => p.TransactionReference).HasMaxLength(100);
        builder.Property(p => p.FailureReason).HasMaxLength(500);
        builder.Property(p => p.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(p => p.CreatedAt).IsRequired();

        // Ignore domain events
        builder.Ignore(p => p.DomainEvents);
    }
}
