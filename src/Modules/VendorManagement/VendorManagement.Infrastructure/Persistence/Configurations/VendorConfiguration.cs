using VendorManagement.Domain.Aggregates;
using VendorManagement.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VendorManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Vendor aggregate.
/// </summary>
public sealed class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("Vendors");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .HasConversion(
                id => id.Value,
                value => VendorId.Create(value))
            .HasColumnName("Id");

        builder.Property(v => v.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.TaxId)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(v => v.TaxId).IsUnique();

        builder.Property(v => v.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Configure ContactInfo value object
        builder.OwnsOne(v => v.Contact, contact =>
        {
            contact.Property(c => c.Email).HasColumnName("Email").IsRequired().HasMaxLength(255);
            contact.Property(c => c.Phone).HasColumnName("Phone").HasMaxLength(50);
            contact.Property(c => c.ContactPerson).HasColumnName("ContactPerson").HasMaxLength(100);
        });

        // Configure Address value object
        builder.OwnsOne(v => v.Address, address =>
        {
            address.Property(a => a.Street).HasColumnName("Street").HasMaxLength(200);
            address.Property(a => a.City).HasColumnName("City").HasMaxLength(100);
            address.Property(a => a.State).HasColumnName("State").HasMaxLength(100);
            address.Property(a => a.PostalCode).HasColumnName("PostalCode").HasMaxLength(20);
            address.Property(a => a.Country).HasColumnName("Country").HasMaxLength(100);
        });

        builder.Property(v => v.BankAccountNumber).HasMaxLength(50);
        builder.Property(v => v.BankRoutingNumber).HasMaxLength(50);
        builder.Property(v => v.PaymentTermDays).IsRequired();
        builder.Property(v => v.Notes).HasMaxLength(2000);
        builder.Property(v => v.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(v => v.CreatedAt).IsRequired();
        builder.Property(v => v.ActivatedBy).HasMaxLength(100);

        builder.Ignore(v => v.DomainEvents);
    }
}
