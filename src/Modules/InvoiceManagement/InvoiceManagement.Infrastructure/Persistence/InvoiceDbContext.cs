using InvoiceManagement.Domain.Aggregates;
using InvoiceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Primitives;
using Shared.Infrastructure.Outbox;
using Shared.Infrastructure.Interceptors;

namespace InvoiceManagement.Infrastructure.Persistence;

/// <summary>
/// DbContext for Invoice Management bounded context.
/// Each module has its own DbContext following DDD module autonomy.
/// </summary>
public sealed class InvoiceDbContext : DbContext, IUnitOfWork
{
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLineItem> LineItems => Set<InvoiceLineItem>();

    public InvoiceDbContext(DbContextOptions<InvoiceDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("invoice");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InvoiceDbContext).Assembly);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new ConvertDomainEventsToOutboxMessagesInterceptor());
        base.OnConfiguring(optionsBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before saving
        var domainEvents = ChangeTracker.Entries<AggregateRoot<InvoiceManagement.Domain.ValueObjects.InvoiceId>>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        // Clear domain events
        foreach (var entry in ChangeTracker.Entries<AggregateRoot<InvoiceManagement.Domain.ValueObjects.InvoiceId>>())
        {
            entry.Entity.ClearDomainEvents();
        }

        var result = await base.SaveChangesAsync(cancellationToken);
        return result;
    }
}
