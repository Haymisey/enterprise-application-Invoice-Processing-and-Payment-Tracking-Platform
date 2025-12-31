using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PaymentTracking.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for PaymentDbContext.
/// This allows Entity Framework Core tools to create an instance of the DbContext
/// during design-time operations (e.g., migrations).
/// </summary>
public sealed class PaymentDbContextFactory : IDesignTimeDbContextFactory<PaymentDbContext>
{
    public PaymentDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PaymentDbContext>();
        
        // Default connection string for design-time (can be overridden via environment variable)
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PaymentDb")
            ?? "Server=localhost,1433;Database=InvoiceProcessingPlatform;User Id=sa;Password=Invoice@SecurePass123!;TrustServerCertificate=True";

        optionsBuilder.UseSqlServer(
            connectionString,
            sqlOptions =>
            {
                sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "payment");
                sqlOptions.EnableRetryOnFailure(3);
            });

        return new PaymentDbContext(optionsBuilder.Options);
    }
}


