using VendorManagement.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Primitives;
using Shared.Infrastructure.Outbox;
using Shared.Infrastructure.Interceptors;

namespace VendorManagement.Infrastructure.Persistence;

/// <summary>
/// DbContext for Vendor Management bounded context.
/// </summary>
public sealed class VendorDbContext : DbContext, IUnitOfWork
{
    public DbSet<Vendor> Vendors => Set<Vendor>();

    public VendorDbContext(DbContextOptions<VendorDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("vendor");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VendorDbContext).Assembly);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new ConvertDomainEventsToOutboxMessagesInterceptor());
        base.OnConfiguring(optionsBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker.Entries<AggregateRoot<VendorManagement.Domain.ValueObjects.VendorId>>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        foreach (var entry in ChangeTracker.Entries<AggregateRoot<VendorManagement.Domain.ValueObjects.VendorId>>())
        {
            entry.Entity.ClearDomainEvents();
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        return result;
    }
}
