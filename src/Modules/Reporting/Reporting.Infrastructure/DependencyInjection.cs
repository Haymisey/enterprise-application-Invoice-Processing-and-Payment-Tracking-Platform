using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reporting.Infrastructure.Persistence;
using Reporting.Infrastructure.EventConsumers;

namespace Reporting.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddReportingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ReportingDb");
        
        services.AddDbContext<ReportingDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString);
        });

        services.AddHostedService<ReportingEventConsumer>();

        return services;
    }
}
