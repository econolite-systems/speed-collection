using Microsoft.Extensions.DependencyInjection;
using Econolite.Ode.Monitoring.Metrics.Extensions;
using Econolite.Ode.Persistence.Mongo;
using Status.Common.Messaging.Extensions;
using SegmentCollection.Services.Cache;
using SegmentCollection.Repository.Interfaces;
using SegmentCollection.Repository;
using SegmentCollection.Services.Database;

namespace SegmentCollection.Extensions
{
    public static class Defined
    {
        //Need to add direct project references
        public static IServiceCollection AddLongQueueServices(this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            services
                .AddMetrics(configuration, "LongQueueSpeedEvent")
                .Configure<SegmentCollectionCacheOptions>(_ => _.CacheRefreshPeriod = TimeSpan.FromMinutes(int.Parse(configuration["ConfigurationCacheRefreshPeriodMinutes"] ?? "15")))
                .AddMemoryCache()
                .AddMongo();

            services.AddActionEventStatusSink(configuration);
            services.AddScoped<ISegmentCollectionSegmentService, SegmentCollectionSegmentService>();
            services.AddScoped<ISegmentCollectionRepository, SegmentCollectionRepository>();

            return services;
        }

    }
}
