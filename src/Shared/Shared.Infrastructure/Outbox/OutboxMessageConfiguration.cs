using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.Infrastructure.Outbox;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(x => x.Content)
            .IsRequired();
            
        builder.Property(x => x.OccurredOnUtc)
            .IsRequired();
            
        builder.Property(x => x.ProcessedOnUtc)
            .IsRequired(false);
            
        builder.Property(x => x.Error)
            .HasMaxLength(2000)
            .IsRequired(false);
            
        // Index for querying unprocessed messages
        builder.HasIndex(x => new { x.ProcessedOnUtc, x.OccurredOnUtc });
    }
}