using WineFilesApi.Application.Interfaces;
using WineFilesApi.Application.Services;
using WineFilesApi.Infrastructure.Data;
using WineFilesApi.Infrastructure.Repositories;

namespace WineFilesApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<FoxProConnectionFactory>();

        services.AddScoped<IBatchRepository, BatchRepository>();
        services.AddScoped<IBlendRepository, BlendRepository>();
        services.AddScoped<IPmDbfRepository, PmDbfRepository>();
        services.AddScoped<IPmFieldRepository, PmFieldRepository>();
        services.AddScoped<IPmIndexRepository, PmIndexRepository>();

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBatchService, BatchService>();
        services.AddScoped<IBlendService, BlendService>();
        services.AddScoped<IPmDbfService, PmDbfService>();
        services.AddScoped<IPmFieldService, PmFieldService>();
        services.AddScoped<IPmIndexService, PmIndexService>();

        return services;
    }

    public static IServiceCollection AddDefaultCors(
        this IServiceCollection services, IConfiguration configuration)
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

    public static IServiceCollection AddWineFilesApi(
        this IServiceCollection services, IConfiguration configuration)
        => services.AddInfrastructure()
                   .AddApplication()
                   .AddDefaultCors(configuration);
}