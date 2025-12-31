using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InvoiceManagement.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for InvoiceDbContext.
/// This allows Entity Framework Core tools to create an instance of the DbContext
/// during design-time operations (e.g., migrations).
/// </summary>
public sealed class InvoiceDbContextFactory : IDesignTimeDbContextFactory<InvoiceDbContext>
{
    public InvoiceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InvoiceDbContext>();
        
        // Default connection string for design-time (can be overridden via environment variable)
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__InvoiceDb")
            ?? "Server=localhost,1433;Database=InvoiceProcessingPlatform;User Id=sa;Password=Invoice@SecurePass123!;TrustServerCertificate=True";

        optionsBuilder.UseSqlServer(
            connectionString,
            sqlOptions =>
            {
                sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "invoice");
                sqlOptions.EnableRetryOnFailure(3);
            });

        return new InvoiceDbContext(optionsBuilder.Options);
    }
}


