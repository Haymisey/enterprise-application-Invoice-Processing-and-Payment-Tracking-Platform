using AIClassification.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Primitives; 

namespace AIClassification.Infrastructure.Persistence;

public sealed class ClassificationDbContext : DbContext, IUnitOfWork
{
    public ClassificationDbContext(DbContextOptions<ClassificationDbContext> options)
        : base(options)
    {
    }

    public DbSet<InvoiceClassification> InvoiceClassifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Automatically finds the Configuration file we wrote in Step 3.3
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClassificationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}