using Econolite.Ode.Monitoring.HealthChecks.Mongo.Extensions;
using Econolite.Ode.Monitoring.Metrics.Extensions;
using Econolite.Ode.Persistence.Mongo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Econolite.Ode.Extensions.AspNet;
using Status.Common.Messaging.Extensions;
using Econolite.Ode.Authorization.Extensions;
using SegmentCollection.Repository.Interfaces;
using SegmentCollection.Repository;
using SegmentCollection.Services.Cache;
using SegmentCollection.Services.Database;
using EntityService.ServiceOptions;
using AcyclicaConfigRepository.Extensions.Acyclica;
using SegmentCollection.Worker;
using AcyclicaService.Services.Api;
using AcyclicaService.Services.Cache;

await AppBuilder.BuildAndRunWebHostAsync(args, options => { options.Source = "Segment Collection"; }, (builder, services) =>
{
    services.AddHttpClient<IAcyclicaApiService, AcyclicaApiService>(_ =>
    {
        _.BaseAddress = new Uri(builder.Configuration["Services:AcyclicaApi"] ?? "Services:AcyclicaApi missing from configuration");
    });
    services.AddMetrics(builder.Configuration, "LongQueueSpeedEvent")
        .Configure<SegmentCollectionCacheOptions>(_ => _.CacheRefreshPeriod = TimeSpan.FromMinutes(int.Parse(builder.Configuration["ConfigurationCacheRefreshPeriodMinutes"] ?? "15")))
        .AddMemoryCache()
        .AddAcyclicaConfigRepository()
        .AddMongo();

    services.AddEntitiesConfigServiceCall(builder.Configuration);
    services.AddActionEventStatusSink(builder.Configuration);
    services.AddScoped<ISegmentCollectionSegmentService, SegmentCollectionSegmentService>();
    services.AddScoped<ISegmentCollectionRepository, SegmentCollectionRepository>();
    services.AddScoped<IServiceOptionsCache, ServiceOptionsCache>();
    services.AddScoped<ISegmentCollectionCache, SegmentCollectionStaticCache>();
    services.AddScoped<IServiceOptionsSegmentCollectionCache, ServiceOptionsCacheSegmentCollection>();

    services.AddHostedService<Worker>();

    services.AddTokenHandler(options =>
    {
        options.Authority = builder.Configuration.GetValue("Authentication:Authority", "")!;
        options.ClientId = builder.Configuration.GetValue("Authentication:ClientId", "")!;
        options.ClientSecret = builder.Configuration.GetValue("Authentication:ClientSecret", "")!;
    });
}, (builder, checksBuilder) => checksBuilder.AddMongoDbHealthCheck());
