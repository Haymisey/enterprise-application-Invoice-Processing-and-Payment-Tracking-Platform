using PaymentTracking.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Primitives;
using Shared.Infrastructure.Outbox;
using Shared.Infrastructure.Interceptors;

namespace PaymentTracking.Infrastructure.Persistence;

/// <summary>
/// DbContext for Payment Tracking bounded context.
/// Each module has its own DbContext following DDD module autonomy.
/// </summary>
public sealed class PaymentDbContext : DbContext, IUnitOfWork
{
    public DbSet<Payment> Payments => Set<Payment>();

    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("payment");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
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
        var domainEvents = ChangeTracker.Entries<AggregateRoot<PaymentTracking.Domain.ValueObjects.PaymentId>>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        // Clear domain events
        foreach (var entry in ChangeTracker.Entries<AggregateRoot<PaymentTracking.Domain.ValueObjects.PaymentId>>())
        {
            entry.Entity.ClearDomainEvents();
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        return result;
    }
}
