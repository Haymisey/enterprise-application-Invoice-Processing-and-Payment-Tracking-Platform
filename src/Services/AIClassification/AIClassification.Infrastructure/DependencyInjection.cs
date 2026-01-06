using AIClassification.Application.Services;
using AIClassification.Domain.Repositories;
using AIClassification.Infrastructure.AI;
using AIClassification.Infrastructure.Persistence;
using AIClassification.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.Primitives; // Checks for IUnitOfWork

namespace AIClassification.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Database Setup
        var connectionString = configuration.GetConnectionString("Database");
        
        services.AddDbContext<ClassificationDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString);
        });

        // 2. Register Repositories
        services.AddScoped<IClassificationRepository, ClassificationRepository>();
        
        // Register UnitOfWork (DbContext implements it implicitly in many Clean Arch patterns, 
        // but often we need a wrapper. For simplicity, we can register Context as UnitOfWork 
        // OR if you have a specific UnitOfWork implementation in Shared, use that.
        // For this specific guide, we often cast Context to IUnitOfWork in handlers:
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ClassificationDbContext>());

        // 3. Register AI Service
        services.AddScoped<IGeminiService, GeminiService>();

        return services;
    }
}

// Add this small helper to the same file (or a separate one) to make DbContext satisfy IUnitOfWork
// IF your Shared.Domain IUnitOfWork is compatible.
// If Shared.Domain.Primitives.IUnitOfWork exists, we can extend DbContext:
// Note: Usually DbContext already has SaveChangesAsync.