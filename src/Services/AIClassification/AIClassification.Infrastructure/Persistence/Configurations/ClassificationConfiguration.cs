using AIClassification.Domain.Aggregates;
using AIClassification.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIClassification.Infrastructure.Persistence.Configurations;

public sealed class ClassificationConfiguration : IEntityTypeConfiguration<InvoiceClassification>
{
    public void Configure(EntityTypeBuilder<InvoiceClassification> builder)
    {
        builder.ToTable("InvoiceClassifications");

        builder.HasKey(c => c.Id);

        // Convert the ID ValueObject to a Guid for the database
        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => ClassificationId.Create(value));

        builder.Property(c => c.ImageUrl).HasMaxLength(500).IsRequired();
        
        // Save Enum as an integer
        builder.Property(c => c.Status).HasConversion<int>();

        // "OwnsOne" means ExtractedData columns (InvoiceNumber, Amount) 
        // will be stored in the SAME table as the Classification
        builder.OwnsOne(c => c.ExtractedData, data =>
        {
            data.Property(d => d.InvoiceNumber).HasMaxLength(100).IsRequired(false);
            data.Property(d => d.VendorName).HasMaxLength(200).IsRequired(false);
            data.Property(d => d.TotalAmount).HasColumnType("decimal(18,2)");
            data.Property(d => d.Currency).HasMaxLength(10);
            
            // Save List<string> as a single string separated by semicolons
            data.Property(d => d.LineItems)
                .HasConversion(
                    v => string.Join(";", v),
                    v => v.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList());
        });
    }
}