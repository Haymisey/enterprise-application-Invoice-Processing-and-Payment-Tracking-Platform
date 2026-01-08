using InvoiceManagement.Domain.Repositories;
using InvoiceManagement.Infrastructure.Persistence;
using InvoiceManagement.Infrastructure.Persistence.Repositories;
using InvoiceManagement.Infrastructure.EventConsumers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.Primitives;

namespace InvoiceManagement.Infrastructure;

/// <summary>
/// Dependency injection configuration for Invoice Management module.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInvoiceManagementInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure DbContext
        services.AddDbContext<InvoiceDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("InvoiceDb"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "invoice");
                    sqlOptions.EnableRetryOnFailure(3);
                }));

        // Register repositories
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        
        // Register Unit of Work for Invoice module
        services.AddScoped<IInvoiceUnitOfWork>(sp => sp.GetRequiredService<InvoiceDbContext>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<InvoiceDbContext>());
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<InvoiceDbContext>());

        // Register Event Consumers
        services.AddHostedService<InvoiceExtractedEventConsumer>();
        services.AddHostedService<SuspiciousInvoiceEventConsumer>();

        return services;
    }
}
