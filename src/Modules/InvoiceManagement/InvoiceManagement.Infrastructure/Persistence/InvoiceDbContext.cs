using InvoiceManagement.Domain.Aggregates;
using InvoiceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Primitives;

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

        // TODO: Publish domain events to message broker after save
        // This is where the Transactional Outbox pattern would be implemented

        return result;
    }
}
