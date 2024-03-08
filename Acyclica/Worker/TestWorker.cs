using System.Text.Json;
using AcyclicaService.Extensions;
using AcyclicaService.Helpers;
using AcyclicaService.Models.LocationInventory;
using AcyclicaService.Models.Segments;
using AcyclicaService.Models.TravelTime;
using AcyclicaService.Services.Api;
using AcyclicaService.Services.Cache;
using AcyclicaService.Services.Database;
using Econolite.Ode.Authorization;
using Econolite.Ode.Messaging;
using Econolite.Ode.Monitoring.Metrics;
using Econolite.Ode.Service.SpeedStatus;
using Econolite.Ode.Status.Common;
using Econolite.Ode.Status.Speed;
using EntityService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AcyclicaService.Worker
{
    public class TestWorker : BackgroundService
    {
        private readonly IAcyclicaApiService _acyclicaService;
        private readonly ISink<ActionEventStatus> _actionEventStatusSink;
        private readonly IServiceOptionsCache _serviceOptionsCache;
        private readonly ISdk<IEntitiesClient> _entitiesClient;
        private readonly ITokenHandler _tokenHandler;
        private readonly ILogger _logger;
        private readonly Guid _tenantId;
        private readonly IMetricsCounter _messageCounter;

        private readonly ISegmentInventoryService _segmentInventoryService;
        private readonly ILocationInventoryService _locationInventoryService;
        private readonly ISegmentTravelDataService _travelDataService;
        private readonly ISpeedStatusRedisService _speedStatusRedisService;
        public TestWorker(ISink<ActionEventStatus> actionEventStatusSink, IServiceProvider serviceProvider,
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

            _segmentInventoryService = serviceScope.ServiceProvider.GetRequiredService<ISegmentInventoryService>();
            _locationInventoryService = serviceScope.ServiceProvider.GetRequiredService<ILocationInventoryService>();
            _travelDataService = serviceScope.ServiceProvider.GetRequiredService<ISegmentTravelDataService>();
            _acyclicaService = serviceScope.ServiceProvider.GetRequiredService<IAcyclicaApiService>();
            _serviceOptionsCache = serviceScope.ServiceProvider.GetRequiredService<IServiceOptionsCache>();
            _speedStatusRedisService = serviceScope.ServiceProvider.GetRequiredService<ISpeedStatusRedisService>();

        }


        public async Task ExecutePublicAsync(CancellationToken stoppingToken)
        {
            await ExecuteAsync(stoppingToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var token = await _tokenHandler.GetTokenAsync(stoppingToken);
                var client = _entitiesClient.CreateClient(token);
                List<SegmentInventory> segmentInventories = new List<SegmentInventory>
{
    new SegmentInventory
    {
        Id = Guid.NewGuid(),
        SegmentId = 1,
        StartSerial = 100000,
        StartId = 1,
        EndSerial = 200000,
        EndId = 2,
        Distance = 100.0,
        Thresholds = new List<List<Threshold>>(),
        Polyline = "u~lbGp~zyN}IHwB?kKNeGHeGBgVZgDHwDTmDZeGX",
        BaseOffset = 0,
        StartLocationId = 1,
        StartLocationName = "Location1",
        EndLocationName = "Location2",
        EndLocationId = 2
    },
    new SegmentInventory
    {
        Id = Guid.NewGuid(),
        SegmentId = 2,
        StartSerial = 200001,
        StartId = 2,
        EndSerial = 300000,
        EndId = 3,
        Distance = 200.0,
        Thresholds = new List<List<Threshold>>(),
        Polyline = "izrbGjg{yN}C@uCCuBIoFCkQXmYd@}V^",
        BaseOffset = 1,
        StartLocationId = 2,
        StartLocationName = "Location2",
        EndLocationName = "Location3",
        EndLocationId = 3
    },
    new SegmentInventory
    {
        Id = Guid.NewGuid(),
        SegmentId = 3,
        StartSerial = 300001,
        StartId = 3,
        EndSerial = 400000,
        EndId = 4,
        Distance = 300.0,
        Thresholds = new List<List<Threshold>>(),
        Polyline = "}subGpl{yNvcA_BjF@rADdCFlB@dBA",
        BaseOffset = 2,
        StartLocationId = 3,
        StartLocationName = "Location3",
        EndLocationName = "Location4",
        EndLocationId = 4
    }
};

                _logger.LogInformation("Creating test objects");
                Location locations = new Location
                {
                    Locations = new List<LocationInventory>
    {
        new LocationInventory
        {
            UniqueId = Guid.NewGuid(),
            LocationId = 2,
            Latitude = 40.7128,
            Longitude = -74.0060,
            DiffrfSensors = new List<int> {1, 2, 3},
            Cabinets = new List<int> {1, 2, 3},
            VsoSensors = new List<int> {1, 2, 3},
            UserFiles = new List<int> {1, 2, 3}
        },
        new LocationInventory
        {
            UniqueId = Guid.NewGuid(),
            LocationId = 3,
            Latitude = 34.0522,
            Longitude = -118.2437,
            DiffrfSensors = new List<int> {4, 5, 6},
            Cabinets = new List<int> {4, 5, 6},
            VsoSensors = new List<int> {4, 5, 6},
            UserFiles = new List<int> {4, 5, 6}
        },
        new LocationInventory
        {
            UniqueId = Guid.NewGuid(),
            LocationId = 4,
            Latitude = 41.8781,
            Longitude = -87.6298,
            DiffrfSensors = new List<int> {7, 8, 9},
            Cabinets = new List<int> {7, 8, 9},
            VsoSensors = new List<int> {7, 8, 9},
            UserFiles = new List<int> {7, 8, 9}
        }
    }
                };
                _logger.LogInformation("Getting existing test entities");
                var existingSegmentsCollection = (await client.AllAsync()).ToList();
                foreach (var segment in segmentInventories)
                {
                    var starTimeEpoch = AcyclicaServiceExtensions.GetStartTimeEpoch(300);
                    var endTimeEpoch = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                    var endLocation = locations.Locations.FirstOrDefault(l => l.LocationId == segment.EndLocationId) ??
                                      new Models.LocationInventory.LocationInventory();
                    _logger.LogInformation("Decoding Polylines");
                    var decodedPolyline = GooglePoints.Decode(segment.Polyline);

                    _logger.LogInformation("creating entities");
                    var entityNode = new EntityNode()
                    {
                        Name = $"Segment Id {segment.SegmentId}",
                        Password = "",
                        Username = "",
                        IpAddress = "",
                        Description = "",
                        PrivacyPhrase = "",

                        Type = new EntityTypeId()
                        {
                            Name = "Speed Segment",
                        },
                        Parents = new List<Guid>
                        {

                        },
                        Children = new List<Entity>
                        {

                        },
                        Geometry = new GeoJsonGeometry()
                        {
                            LineString = new GeoJsonLineStringFeature()
                            {
                                Coordinates = new double[][]
                                   {
                                        new double[] { decodedPolyline.First()[0], decodedPolyline.First()[1] }
                                   }
                            }
                        },
                        IdMapping = segment.SegmentId,
                        IsLeaf = false
                    };

                    var existingEntity = existingSegmentsCollection.Where(x => x.IdMapping == segment.SegmentId).FirstOrDefault();
                    if (existingEntity != null)
                    {
                        _logger.LogInformation("Updating an existing entity");
                        entityNode.Id = existingEntity.Id;
                        var entityJson = JsonSerializer.Serialize(entityNode);
                        await client.EntitiesPUTAsync(entityNode, stoppingToken);
                        _logger.LogInformation("Entity Updated");

                    }
                    else
                    {
                        _logger.LogInformation("Creating a new entity");
                        var entityJson = JsonSerializer.Serialize(entityNode);
                        await client.EntitiesPOSTAsync(entityNode, stoppingToken);
                        _logger.LogInformation("Entity Added");
                    }
                    var speedEvent = new SpeedEvent()
                    {
                        CommStatus = CommStatus.Unknown,
                        SegmentId = segment.SegmentId,
                        Latitude = endLocation.Latitude,
                        Longitude = endLocation.Longitude
                    };


                    try
                    {
                        //Get segment travel data which has the average travel time for each segment
                        List<TravelData> travelDataList = new List<TravelData>
{
    new TravelData
    {
        Id = Guid.NewGuid(),
        SegmentId = 1,
        Time = 1616161616,
        Strength = 10,
        First = 1,
        Last = 10,
        Minimum = 1,
        Maximum = 10
    },
    new TravelData
    {
        Id = Guid.NewGuid(),
        SegmentId = 2,
        Time = 1717171717,
        Strength = 20,
        First = 11,
        Last = 20,
        Minimum = 11,
        Maximum = 20
    },
    new TravelData
    {
        Id = Guid.NewGuid(),
        SegmentId = 3,
        Time = 1818181818,
        Strength = 30,
        First = 21,
        Last = 30,
        Minimum = 21,
        Maximum = 30
    }
};

                        foreach (var travelData in travelDataList)
                        {
                            await _travelDataService.UpdateAsync(travelData.ToDoc());
                            _logger.LogInformation($"Travel Data Entity updated at: {DateTimeOffset.Now}");

                            //Using the travel time with the segment distance to calculate the average speed for that area.
                            var segmentSpeed =
                                AcyclicaServiceExtensions.ConvertSegmentTravelTimeFromMillisecondsIntoMph(
                                    segment.Distance, travelData.Strength);
                            speedEvent.SegmentSpeed = segmentSpeed;
                            await _speedStatusRedisService.PublishAsync(_tenantId, speedEvent);
                            _logger.LogInformation(
                                $"Incrementing message counter and publishing speed to Kafka Queue ");
                            _messageCounter.Increment();
                        }
                    }
                    catch (Exception)
                    {
                        speedEvent.CommStatus = CommStatus.Bad;

                        _logger.LogError(
                            $"An error occurred while processing segment {segment.SegmentId}. Continuing with the next segment.");
                    }

                    //   await _actionEventStatusSink.SinkAsync(_tenantId, speedEvent, stoppingToken);

                }
                //   await Task.Delay(TimeSpan.FromMinutes(300), stoppingToken);
                _logger.LogInformation("Test completed");
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
}
