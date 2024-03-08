using AcyclicaService.Helpers;
using AcyclicaService.Services.Api;
using AcyclicaService.Services.Cache;
using Econolite.Ode.Authorization;
using Econolite.Ode.Messaging;
using Econolite.Ode.Monitoring.Metrics;
using Econolite.Ode.Status.Common;
using Econolite.Ode.Status.CorridorSpeedEvent;
using EntityService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SegmentCollection.Extensions;
using SegmentCollection.Services.Cache;
using SegmentCollection.Services.Database;

namespace SegmentCollection.Worker
{
    public class Worker : BackgroundService
    {
        private readonly IAcyclicaApiService _acyclicaService;
        private readonly IServiceOptionsCache _serviceOptionsCache;
        private readonly ISink<ActionEventStatus> _actionEventStatusSink;
        private readonly ISdk<IEntitiesClient> _entitiesClient;
        private readonly ITokenHandler _tokenHandler;
        private readonly ILogger _logger;
        private readonly string _topic;
        private readonly int _segmentCollectionLength;
        private readonly Guid _tenantId;
        private readonly IMetricsCounter _messageCounter;

        private readonly ISegmentCollectionSegmentService _segmentCollectionSegmentService;

        public Worker(IServiceProvider serviceProvider, ISink<ActionEventStatus> actionEventStatusSink,
            ILoggerFactory loggerFactory, IConfiguration configuration,
            IMetricsFactory metricsFactory, ISdk<IEntitiesClient> entitiesClient, ITokenHandler tokenHandler
        )
        {
            _actionEventStatusSink = actionEventStatusSink;
            _entitiesClient = entitiesClient;
            _tokenHandler = tokenHandler;
            _logger = loggerFactory.CreateLogger(GetType().Name);
            _tenantId = Guid.Parse(configuration["Tenant:Id"] ??
                                   throw new NullReferenceException("Tenant:Id missing in config."));
            _segmentCollectionLength = int.Parse(configuration["SegmentCollectionLength"] ?? "3");
            _topic = configuration["Topics:speedevents"] ?? throw new NullReferenceException("Topics:speedevents missing in config.");

            var serviceScope = serviceProvider.CreateScope();
            _serviceOptionsCache = serviceScope.ServiceProvider.GetRequiredService<IServiceOptionsCache>();
            _acyclicaService = serviceScope.ServiceProvider.GetRequiredService<IAcyclicaApiService>();
            _segmentCollectionSegmentService = serviceScope.ServiceProvider.GetRequiredService<ISegmentCollectionSegmentService>(); ;

        }

        public async Task ExecutePublicAsync(CancellationToken stoppingToken)
        {
            await ExecuteAsync(stoppingToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            _logger.LogInformation("Starting the main loop");
                            var token = await _tokenHandler.GetTokenAsync(stoppingToken);
                            var client = _entitiesClient.CreateClient(token);
                            var serviceOptions = await _serviceOptionsCache.GetOptionsAsync();
                            while (!stoppingToken.IsCancellationRequested)
                            {
                                try
                                {
                                    token = await _tokenHandler.GetTokenAsync(stoppingToken);
                                    client.Token = token;
                                    var sortedEntityNodes = new List<EntityNode>();

                                    var entityType = (await client.EntityTypeAllAsync(stoppingToken)).Where(x => x.Name.Contains("Speed Segment")).FirstOrDefault();
                                    if (entityType == null)
                                    {
                                        continue;
                                    }
                                    var segmentEntities = (await client.Types2Async(entityType.Name, stoppingToken)).Where(x => x.Geometry?.LineString?.Coordinates != null && x.Geometry?.LineString?.Coordinates.Count > 0).ToList();

                                    //Sort by lat and then long
                                    sortedEntityNodes = segmentEntities.OrderBy(en => en.Geometry.LineString.Coordinates.First().ToArray()[0])
                                    .ThenBy(en => en.Geometry.LineString.Coordinates.First().ToArray()[1]).ToList();

                                    var starTimeEpoch = AcyclicaServiceExtensions.GetStartTimeEpoch(serviceOptions.PollingRate);
                                    var endTimeEpoch = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

                                    //Before doing long segments, put the speed value information into each entity since it cannot be done in the previous worker
                                    foreach (var node in sortedEntityNodes)
                                    {

                                        //Get segment travel data which has the average travel time for each segment
                                        var travelDataList = await _acyclicaService.GetSegmentTravelDataAsync("json",
                                         node.IdMapping.Value,
                                         starTimeEpoch, endTimeEpoch, serviceOptions.PollingRate * 1000);

                                        //Using the travel time with the segment distance to calculate the average speed for that area.
                                        if (node.Geometry?.LineString?.Properties != null && travelDataList.Count > 0)
                                        {
                                            var distance = node.Geometry.LineString.Properties.TripPointLocations.FirstOrDefault()?.Point.FirstOrDefault() ?? 0;
                                            var time = travelDataList.FirstOrDefault()?.Strength ?? 0;
                                            var segmentSpeed = AcyclicaServiceExtensions.ConvertSegmentTravelTimeFromMillisecondsIntoMph(distance, time);
                                            node.Geometry.LineString.Properties.SpeedLimit = segmentSpeed;
                                        }
                                        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                                    }

                                    try
                                    {
                                        var longSegments =
                                            SegmentCollectionServiceExtensions.CreateLongQueueFromSegments(sortedEntityNodes,
                                                _segmentCollectionLength);

                                        foreach (var longSegment in longSegments)
                                        {
                                            var longSegmentEvent = new CorridorSegmentSpeedEvent()
                                            {
                                                SegmentSpeed = longSegment.SegmentCollectionSpeed
                                            };
                                            _logger.LogInformation($" Created corridor segment event {longSegmentEvent}");
                                            await _actionEventStatusSink.SinkAsync(_tenantId, longSegmentEvent, stoppingToken);
                                            _logger.LogInformation($" Sinking corridor segment event into producer {longSegmentEvent}");
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        _logger.LogError($"An error occurred while processing the segment collection.");
                                    }

                                    await Task.Delay(TimeSpan.FromSeconds(serviceOptions.PollingRate), stoppingToken);
                                }
                                catch (ApiException ex)
                                {
                                    _logger.LogInformation($"The HTTP status code of the response was not expected for the client call with url {client.BaseUrl}");
                                }
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            _logger.LogInformation($"The Execution was canceled");
                        }
                        catch (Exception Ex)
                        {
                            _logger.LogCritical(Ex, "");
                        }
                        finally
                        {
                            _logger.LogInformation("Ending the main loop");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Acyclica Segment Collection Worker process stopped unexpectedly\n" + ex.StackTrace);
                    throw;
                }
                _logger.LogInformation("Acyclica Segment Collection Worker maintainer stopped");
            });
        }
    }
}