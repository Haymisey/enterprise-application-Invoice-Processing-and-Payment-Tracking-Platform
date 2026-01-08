using VendorManagement.Domain.Aggregates;
using VendorManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Primitives;
using Shared.Infrastructure.Outbox;
using Shared.Infrastructure.Interceptors;

namespace VendorManagement.Infrastructure.Persistence;

/// <summary>
/// DbContext for Vendor Management bounded context.
/// </summary>
public sealed class VendorDbContext : DbContext, IVendorUnitOfWork
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
}
