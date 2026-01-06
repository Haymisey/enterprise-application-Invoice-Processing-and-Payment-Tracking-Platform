using VendorManagement.Domain.Repositories;
using VendorManagement.Infrastructure.Persistence;
using VendorManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.Primitives;

namespace VendorManagement.Infrastructure;

/// <summary>
/// Dependency injection configuration for Vendor Management module.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddVendorManagementInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<VendorDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("VendorDb"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "vendor");
                    sqlOptions.EnableRetryOnFailure(3);
                }));

        services.AddScoped<IVendorRepository, VendorRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<VendorDbContext>());
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<VendorDbContext>());

        return services;
    }
}
