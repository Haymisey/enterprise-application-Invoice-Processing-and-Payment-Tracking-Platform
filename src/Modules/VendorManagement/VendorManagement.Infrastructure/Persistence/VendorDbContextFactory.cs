using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VendorManagement.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for VendorDbContext.
/// This allows Entity Framework Core tools to create an instance of the DbContext
/// during design-time operations (e.g., migrations).
/// </summary>
public sealed class VendorDbContextFactory : IDesignTimeDbContextFactory<VendorDbContext>
{
    public VendorDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<VendorDbContext>();
        
        // Default connection string for design-time (can be overridden via environment variable)
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__VendorDb")
            ?? "Server=localhost,1433;Database=InvoiceProcessingPlatform;User Id=sa;Password=Invoice@SecurePass123!;TrustServerCertificate=True";

        optionsBuilder.UseSqlServer(
            connectionString,
            sqlOptions =>
            {
                sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "vendor");
                sqlOptions.EnableRetryOnFailure(3);
            });

        return new VendorDbContext(optionsBuilder.Options);
    }
}


