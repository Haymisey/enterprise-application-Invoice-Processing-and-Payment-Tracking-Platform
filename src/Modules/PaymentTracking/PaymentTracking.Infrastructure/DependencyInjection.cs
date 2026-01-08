using PaymentTracking.Domain.Repositories;
using PaymentTracking.Infrastructure.Persistence;
using PaymentTracking.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.Primitives;

namespace PaymentTracking.Infrastructure;

/// <summary>
/// Dependency injection configuration for Payment Tracking module.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddPaymentTrackingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure DbContext
        services.AddDbContext<PaymentDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("PaymentDb"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "payment");
                    sqlOptions.EnableRetryOnFailure(3);
                }));

        // Register repositories
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        
        // Register Unit of Work for Payment Tracking module
        // Register PaymentDbContext as IUnitOfWork
        // NOTE: This registration order matters - PaymentTracking should be registered LAST in Program.cs
        // to ensure PaymentTracking handlers get PaymentDbContext instead of other modules' DbContexts
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PaymentDbContext>());
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<PaymentDbContext>());

        // Register event consumer for inter-module communication
        services.AddHostedService<EventConsumers.InvoiceApprovedEventConsumer>();

        return services;
    }
}
