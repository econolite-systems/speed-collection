using AcyclicaService.Services.Api;
using AcyclicaService.Services.Cache;
using AcyclicaService.Services.Database;
using Econolite.Ode.Authorization;
using Econolite.Ode.Messaging;
using Econolite.Ode.Monitoring.Metrics;
using Econolite.Ode.Status.Common;
using EntityService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SegmentCollection.Worker;

namespace SegmentCollection.Test
{
    public class WorkerTests
    {
        private readonly Mock<ISink<ActionEventStatus>> _mockActionEventStatusSink = new();
        private readonly Mock<IServiceOptionsCache> _mockServiceOptionsCache = new();
        private readonly Mock<ILogger<Worker.Worker>> _mockLogger = new();
        private readonly Mock<ILoggerFactory> _mockLoggerFactory = new();
        private readonly Mock<IMetricsFactory> _mockMetricsFactory = new();
        private readonly Mock<IConfiguration> _mockConfiguration = new();
        private readonly Mock<ISegmentInventoryService> _mockSegmentInventoryService = new();
        private readonly Mock<IAcyclicaApiService> _mockAcylicaService = new();
        private readonly Mock<ILocationInventoryService> _mockLocationInventoryService = new();
        private readonly Mock<ISegmentTravelDataService> _mockTravelDataService = new();
        private readonly Mock<ISdk<IEntitiesClient>> _mockEntitiesClient = new Mock<ISdk<IEntitiesClient>>();
        private readonly Mock<IEntitiesClient> _mockIEntitiesClient = new Mock<IEntitiesClient>();
        private readonly Mock<ITokenHandler> _mockTokenHandler = new Mock<ITokenHandler>();



        [Fact]
        public void Test1()
        {

        }
    }
}