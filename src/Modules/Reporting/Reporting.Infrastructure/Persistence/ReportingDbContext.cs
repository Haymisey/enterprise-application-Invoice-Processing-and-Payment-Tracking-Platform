using Microsoft.EntityFrameworkCore;
using Reporting.Domain.Entities;

namespace Reporting.Infrastructure.Persistence;

public sealed class ReportingDbContext : DbContext
{
    public ReportingDbContext(DbContextOptions<ReportingDbContext> options)
        : base(options)
    {
    }

    public DbSet<InvoiceSummary> InvoiceSummaries { get; set; }
    public DbSet<VendorSpendingSummary> VendorSpendingSummaries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("reporting");
        
        modelBuilder.Entity<InvoiceSummary>(builder =>
        {
            builder.HasKey(x => x.Status);
            builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<VendorSpendingSummary>(builder =>
        {
            builder.HasKey(x => x.VendorId);
            builder.Property(x => x.TotalSpent).HasPrecision(18, 2);
        });

        base.OnModelCreating(modelBuilder);
    }
}
