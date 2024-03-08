using AcyclicaConfigRepository.Extensions.Acyclica;
using AcyclicaConfigRepository.Services;
using AcyclicaService.Repository;
using AcyclicaService.Repository.Interfaces;
using AcyclicaService.Services.Api;
using AcyclicaService.Services.Cache;
using AcyclicaService.Services.Database;
using AcyclicaService.Worker;
using Econolite.Ode.Authorization.Extensions;
using Econolite.Ode.Extensions.AspNet;
using Econolite.Ode.Monitoring.HealthChecks.Mongo.Extensions;
using Econolite.Ode.Monitoring.HealthChecks.Redis.Extensions;
using Econolite.Ode.Monitoring.Metrics.Extensions;
using Econolite.Ode.Persistence.Mongo;
using Econolite.Ode.Repository.SpeedStatus;
using Econolite.Ode.Service.SpeedStatus;
using EntityService.ServiceOptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Status.Common.Messaging.Extensions;

await AppBuilder.BuildAndRunWebHostAsync(args, options => { options.Source = "Acyclica Service"; }, (builder, services) =>
{
    services.AddHttpClient<IAcyclicaApiService, AcyclicaApiService>(_ =>
    {
        _.BaseAddress = new Uri(builder.Configuration["Services:AcyclicaApi"] ?? "Services:AcyclicaApi missing from configuration");
    });
    services
        .AddMetrics(builder.Configuration, "AcyclicaSpeed")
        .Configure<ServiceOptionsCacheOptions>(_ => _.CacheRefreshPeriod = TimeSpan.FromMinutes(int.Parse(builder?.Configuration["ConfigurationCacheRefreshPeriodMinutes"] ?? "15")))
        .AddMemoryCache()
        .AddAcyclicaConfigRepository()
    .AddMongo();

    services.AddEntitiesConfigServiceCall(builder.Configuration);
    services.AddActionEventStatusSink(builder.Configuration);
    services.AddScoped<IAcyclicaConfigService, AcyclicaConfigService>();
    services.AddScoped<IServiceOptionsCache, ServiceOptionsCache>();
    services.AddScoped<IAcyclicaSegmentInventoryRepository, AcyclicaSegmentInventoryRepository>();
    services.AddScoped<IAcyclicaLocationInventoryRepository, AcyclicaLocationInventoryRepository>();
    services.AddScoped<IAcyclicaTravelDataRepository, AcyclicaTravelDataRepository>();
    services.AddScoped<ISegmentInventoryService, SegmentInventoryService>();
    services.AddScoped<ISegmentTravelDataService, TravelDataService>();
    services.AddScoped<ILocationInventoryService, LocationInventoryService>();
    services.AddScoped<ISpeedStatusRepository, SpeedStatusRepository>();
    services.AddScoped<ISpeedStatusRedisService, SpeedStatusRedisService>();

    services.AddHostedService<Worker>();

    //ONLY Uncomment the below when testing and comment the line above
    //  serviceCollection.AddHostedService<TestWorker>();
    services.AddTokenHandler(options =>
    {
        options.Authority = builder.Configuration.GetValue("Authentication:Authority", "")!;
        options.ClientId = builder.Configuration.GetValue("Authentication:ClientId", "")!;
        options.ClientSecret = builder.Configuration.GetValue("Authentication:ClientSecret", "")!;
    });
}, (builder, checksBuilder) => checksBuilder.AddMongoDbHealthCheck().AddRedisHealthCheck(builder.Configuration.GetConnectionString("Redis") ?? throw new NullReferenceException($"ConnectionStrings:Redis missing from config.")));
