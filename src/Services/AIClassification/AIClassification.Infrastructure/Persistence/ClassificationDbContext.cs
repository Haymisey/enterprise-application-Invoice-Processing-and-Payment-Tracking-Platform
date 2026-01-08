using AIClassification.Domain.Aggregates;
using AIClassification.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Primitives;
using Shared.Infrastructure.Outbox;
using Shared.Infrastructure.Interceptors;

namespace AIClassification.Infrastructure.Persistence;

public sealed class ClassificationDbContext : DbContext, IAIUnitOfWork
{
    public ClassificationDbContext(DbContextOptions<ClassificationDbContext> options)
        : base(options)
    {
    }

    public DbSet<InvoiceClassification> InvoiceClassifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("classification");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClassificationDbContext).Assembly);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new ConvertDomainEventsToOutboxMessagesInterceptor());
        base.OnConfiguring(optionsBuilder);
    }
}