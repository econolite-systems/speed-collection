using AcyclicaConfigRepository.Extensions.Acyclica;
using AcyclicaService.Repository;
using AcyclicaService.Repository.Interfaces;
using AcyclicaService.Services.Api;
using AcyclicaService.Services.Cache;
using AcyclicaService.Services.Database;
using Econolite.Ode.Monitoring.Metrics.Extensions;
using Econolite.Ode.Persistence.Mongo;
using Microsoft.Extensions.DependencyInjection;
using Status.Common.Messaging.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAcyclicaServices(this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        services.AddHttpClient<IAcyclicaApiService, AcyclicaApiService>(_ =>
        {
            _.BaseAddress = new Uri(configuration["Services:AcyclicaApi"] ?? "Services:AcyclicaApi missing from configuration");
        });
        services
            .Configure<ServiceOptionsCacheOptions>(_ => _.CacheRefreshPeriod = TimeSpan.FromMinutes(int.Parse(configuration["ConfigurationCacheRefreshPeriodMinutes"] ?? "15")))
            .AddMemoryCache()
            .AddAcyclicaConfigRepository()
            .AddMongo();

        services.AddActionEventStatusSink(configuration);
        services.AddScoped<IServiceOptionsCache, ServiceOptionsCache>();
        services.AddScoped<ISegmentInventoryService, SegmentInventoryService>();
        services.AddScoped<ILocationInventoryService, LocationInventoryService>();
        services.AddScoped<ISegmentTravelDataService, TravelDataService>();
        services.AddScoped<IAcyclicaSegmentInventoryRepository, AcyclicaSegmentInventoryRepository>();
        services.AddScoped<IAcyclicaLocationInventoryRepository, AcyclicaLocationInventoryRepository>();
        services.AddScoped<IAcyclicaTravelDataRepository, AcyclicaTravelDataRepository>();

        return services;
    }
}
