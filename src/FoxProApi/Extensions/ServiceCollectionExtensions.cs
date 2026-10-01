using WineFilesApi.Application.Interfaces;
using WineFilesApi.Application.Services;
using WineFilesApi.Infrastructure.Data;
using WineFilesApi.Infrastructure.Repositories;

namespace WineFilesApi.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers infrastructure services (data access, connection factories).
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        services.AddSingleton<FoxProConnectionFactory>();

        services.AddScoped<IBatchRepository, BatchRepository>();
        services.AddScoped<IBlendRepository, BlendRepository>();
        services.AddScoped<IQueryRepository, QueryRepository>();

        return services;
    }

    /// <summary>
    /// Registers application services (business logic).
    /// </summary>
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IBatchService, BatchService>();
        services.AddScoped<IBlendService, BlendService>();
        services.AddScoped<IQueryService, QueryService>();

        return services;
    }

    /// <summary>
    /// Registers the CORS policy from configuration.
    /// </summary>
    public static IServiceCollection AddDefaultCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var origins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCors", policy =>
            {
                if (origins.Length == 0)
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                else
                    policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
            });
        });

        return services;
    }

    /// <summary>
    /// Registers all FoxPro API services (infrastructure + application + CORS).
    /// </summary>
    public static IServiceCollection AddFoxProApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services
            .AddInfrastructure()
            .AddApplication()
            .AddDefaultCors(configuration);
    }
}