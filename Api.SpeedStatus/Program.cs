// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Extensions.AspNet;
using Econolite.Ode.Monitoring.HealthChecks.Redis.Extensions;
using Econolite.Ode.Repository.SpeedStatus;
using Monitoring.AspNet.Metrics;

using EntityService.ServiceOptions;
using SegmentCollection.Repository;
using SegmentCollection.Repository.Interfaces;
using SegmentCollection.Services.Database;
using Econolite.Ode.Monitoring.Metrics.Extensions;

await AppBuilder.BuildAndRunWebHostAsync(args, options => { options.Source = "Speed Status Api"; options.IsApi = true; }, (builder, services) =>
{
    services.ConfigureRequestMetrics(c =>
    {
        c.RequestCounter = "Requests";
        c.ResponseCounter = "Responses";
    });
    services.AddSpeedStatusRepository();
    services.AddAcyclicaServices(builder.Configuration);
    services.AddEntitiesConfigServiceCall(options =>
    {
        options.Configuration = builder.Configuration["Services:Configuration"] ??
                                throw new NullReferenceException("Services:Configuration missing in config");
    });
    services.AddScoped<ISegmentCollectionRepository, SegmentCollectionRepository>();
    services.AddScoped<ISegmentCollectionSegmentService, SegmentCollectionSegmentService>();

}, (builder, checksBuilder) => checksBuilder.AddRedisHealthCheck(builder.Configuration.GetConnectionString("Redis") ?? throw new NullReferenceException("ConnectionStrings:Redis not found in configuration")), app => app.AddRequestMetrics());
