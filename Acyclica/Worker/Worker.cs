using AcyclicaService.Helpers;
using AcyclicaService.Services.Api;
using AcyclicaService.Services.Cache;
using Econolite.Ode.Messaging;
using Econolite.Ode.Monitoring.Metrics;
using Econolite.Ode.Status.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Econolite.Ode.Authorization;
using EntityService;
using Microsoft.Extensions.DependencyInjection;
using Econolite.Ode.Service.SpeedStatus;

namespace AcyclicaService.Worker
{
    public class Worker : BackgroundService
    {
        private readonly IAcyclicaApiService _acyclicaService;
        private readonly ISink<ActionEventStatus> _actionEventStatusSink;
        private readonly IServiceOptionsCache _serviceOptionsCache;
        private readonly ISdk<IEntitiesClient> _entitiesClient;
        private readonly ITokenHandler _tokenHandler;
        private readonly ILogger _logger;
        private readonly Guid _tenantId;
        private readonly IMetricsCounter _messageCounter;
        private readonly ISpeedStatusRedisService _speedStatusRedisService;
        public Worker(ISink<ActionEventStatus> actionEventStatusSink, IServiceProvider serviceProvider,
    IMetricsFactory metricsFactory, ISdk<IEntitiesClient> entitiesClient, ITokenHandler tokenHandler,
    IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _actionEventStatusSink = actionEventStatusSink;
            _entitiesClient = entitiesClient;
            _tokenHandler = tokenHandler;
            _logger = loggerFactory.CreateLogger(GetType().Name);
            _tenantId = Guid.Parse(configuration["Tenant:Id"] ??
                                    throw new NullReferenceException("Tenant:Id missing in config."));
            _messageCounter = metricsFactory.GetMetricsCounter("Speed Events");

            var serviceScope = serviceProvider.CreateScope();

            _acyclicaService = serviceScope.ServiceProvider.GetRequiredService<IAcyclicaApiService>();
            _serviceOptionsCache = serviceScope.ServiceProvider.GetRequiredService<IServiceOptionsCache>();
            _speedStatusRedisService = serviceScope.ServiceProvider.GetRequiredService<ISpeedStatusRedisService>();
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
                            _logger.LogInformation($"Starting new Iteration.");
                            var token = await _tokenHandler.GetTokenAsync(stoppingToken);
                            var client = _entitiesClient.CreateClient(token);
                            while (!stoppingToken.IsCancellationRequested)
                            {
                                try
                                {
                           

                                    _logger.LogInformation($"Obtaining Token.");

                                    token = await _tokenHandler.GetTokenAsync(stoppingToken);
                                    client.Token = token;
                                    var serviceOptions = await _serviceOptionsCache.GetOptionsAsync();

                                    var entityType = (await client.EntityTypeAllAsync(stoppingToken)).Where(x => x.Name.Contains("Speed Segment")).FirstOrDefault();
                                    if (entityType == null)
                                    {
                                        await Task.Delay(60000);
                                        continue;
                                    }

                                    var parent = (await client.Types2Async("System")).FirstOrDefault();
                                    if (parent == null)
                                    {
                                        await Task.Delay(60000);
                                        continue;
                                    }

                                    var segments = await _acyclicaService.GetSegmentInventoryAsync();


                                    var filteredSegments = segments?.Where(x => !x.Polyline.IsNullOrEmpty()).ToList() ?? new List<Models.Segments.SegmentInventory>();
                                    var locations = await _acyclicaService.GetLocationInventoryAsync();

                                    var existingSegmentsCollection = (await client.Types2Async(entityType.Name)).Where(x => x.IsDeleted == false).ToList();

                                    var entitiesToDelete = existingSegmentsCollection.Where(entity => entity.IdMapping.HasValue && !filteredSegments.Any(id => id.SegmentId == entity.IdMapping)).ToList();

                            

                                    foreach (var entity in entitiesToDelete)
                                    {
                                        await client.EntitiesDELETEAsync(entity.Id, stoppingToken);
                                    }

                                  


                                    var entitiesToAdd = filteredSegments.Where(segment => !existingSegmentsCollection.Any(y => y.IdMapping == segment.SegmentId)).Select(s =>
                                    {
                                        var location = locations.Locations.FirstOrDefault(l => l.LocationId == s.EndLocationId) ??
                                                          new Models.LocationInventory.LocationInventory();
                                        return s.ToEntityNode(entityType, parent, location);
                                    }).ToList();

                                    var entitiesToUpdate = existingSegmentsCollection.Where(entity => entity.IdMapping.HasValue && filteredSegments.Any(id => id.SegmentId == entity.IdMapping)).Select(e =>
                                    {
                                        var segment = filteredSegments.FirstOrDefault(segment => segment.SegmentId == e.IdMapping);
                                        if (segment == null)
                                        {
                                            return null;
                                        }
                                        var location = locations.Locations.FirstOrDefault(l => l.LocationId == segment.EndLocationId) ??
                                                          new Models.LocationInventory.LocationInventory();
                                        return e.HasChanged(segment) ? e.Update(segment, location) : null;
                                    }).Where(e => e != null).ToList();


                                    foreach (var entityToAdd in entitiesToAdd)
                                    {
                                        await client.EntitiesPOSTAsync(entityToAdd, stoppingToken);
                                    }
                                    foreach (var entityToUpdate in entitiesToUpdate)
                                    {
                                        await client.EntitiesPUTAsync(entityToUpdate, stoppingToken);
                                    }
                                 


                                    if (entitiesToAdd.Count > 0)
                                    {
                                        existingSegmentsCollection = (await client.Types2Async(entityType.Name)).Where(x => x.IsDeleted == false).ToList();
                                    }

                                    foreach (var segment in existingSegmentsCollection)
                                    {
                                        if (segment.IdMapping.HasValue == false)
                                        {
                                            continue;
                                        }
                                        var speedEvent = segment.ToSpeedEvent();
                                        var starTimeEpoch = AcyclicaServiceExtensions.GetStartTimeEpoch(serviceOptions.PollingRate);
                                        var endTimeEpoch = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

                                        try
                                        {
                                            //Get segment travel data which has the average travel time for each segment
                                            _logger.LogInformation($"Pulling Travel Data for Speed Segment {speedEvent.SegmentId}.");
                                            var travelDataList = await _acyclicaService.GetSegmentTravelDataAsync("json", segment.IdMapping.Value, starTimeEpoch, endTimeEpoch, serviceOptions.PollingRate * 1000);
                                            _logger.LogInformation($"Pull Successful");

                                            speedEvent.CommStatus = CommStatus.Good;

                                            if (travelDataList.Any(x => x.Strength > 0))
                                            {
                                                foreach (var travelData in travelDataList)
                                                {
                                                    //Using the travel time with the segment distance to calculate the average speed for that area.
                                                    var segmentSpeed = AcyclicaServiceExtensions.ConvertSegmentTravelTimeFromMillisecondsIntoMph(segment.Geometry.LineString.Properties.TripPointLocations.First().Point.First(), travelData.Strength);

                                                    speedEvent.SegmentSpeed = segmentSpeed;

                                                    //Publish to Redis
                                                    try
                                                    {
                                                        await _speedStatusRedisService.PublishAsync(_tenantId, speedEvent);
                                                    }
                                                    catch (Exception)
                                                    {

                                                        _logger.LogError($"An error occurred while pushing speed event with segment id {speedEvent.SegmentId} and speed of {speedEvent.SegmentSpeed} to Redis.");
                                                    }

                                                    _messageCounter.Increment();
                                                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            speedEvent.CommStatus = CommStatus.Bad;

                                            _logger.LogError(ex, $"An error occurred while pulling travel data for segment {segment.IdMapping}. Continuing with the next segment.");
                                            break;
                                        }

                                        await _actionEventStatusSink.SinkAsync(_tenantId, speedEvent, stoppingToken);
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
                    _logger.LogCritical(ex, "Acyclica Worker process stopped unexpectedly\n" + ex.StackTrace);
                    throw;
                }
                _logger.LogInformation("Acyclica Worker maintainer stopped");
            });
        }
    }
}