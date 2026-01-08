using AIClassification.Application.Services;
using AIClassification.Domain.Repositories;
using AIClassification.Infrastructure.AI;
using AIClassification.Infrastructure.Persistence;
using AIClassification.Infrastructure.Persistence.Repositories;
using AIClassification.Infrastructure.EventConsumers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.Primitives; // Checks for IUnitOfWork

namespace AIClassification.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAIClassificationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Database Setup
        var connectionString = configuration.GetConnectionString("ClassificationDb");
        
        services.AddDbContext<ClassificationDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString);
        });

        // 2. Register Repositories
        services.AddScoped<IClassificationRepository, ClassificationRepository>();
        
        // Register UnitOfWork
        services.AddScoped<IAIUnitOfWork>(sp => sp.GetRequiredService<ClassificationDbContext>());
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ClassificationDbContext>());

        // 3. Register AI Service
        services.AddScoped<IGeminiService, GeminiService>();

        // 4. Register Event Consumers
        services.AddHostedService<ClassificationStartedEventConsumer>();

        return services;
    }
}

// Add this small helper to the same file (or a separate one) to make DbContext satisfy IUnitOfWork
// IF your Shared.Domain IUnitOfWork is compatible.
// If Shared.Domain.Primitives.IUnitOfWork exists, we can extend DbContext:
// Note: Usually DbContext already has SaveChangesAsync.