using AcyclicaService.Services.Cache;
using Microsoft.Extensions.Logging;
using AcyclicaService.Services.Api;
using Econolite.Ode.Messaging;
using Econolite.Ode.Monitoring.Metrics;
using Econolite.Ode.Status.Common;
using Econolite.Ode.Status.Speed;
using Microsoft.Extensions.Configuration;
using AcyclicaService.Models.Segments;
using AcyclicaService.Services.Database;
using AcyclicaService.Models.LocationInventory;
using AcyclicaService.Repository;
using Econolite.Ode.Authorization;
using EntityService;

namespace AcyclicaService.Test
{
    public class WorkerTests
    {
        private readonly Mock<IAcyclicaApiService> _mockAcyclicaService = new();
        private readonly Mock<ISink<ActionEventStatus>> _mockActionEventStatusSink = new();
        private readonly Mock<IServiceOptionsCache> _mockServiceOptionsCache;
        private readonly Mock<IServiceProvider> _mockServiceProvider = new();
        private readonly Mock<ILogger<Worker.Worker>> _mockLogger = new();
        private readonly Mock<ILoggerFactory> _mockLoggerFactory = new();
        private readonly Mock<IMetricsFactory> _mockMetricsFactory = new();
        private readonly Mock<IConfiguration> _mockConfiguration = new();
        private readonly Mock<ISegmentInventoryService> _mockSegmentInventoryService = new();
        private readonly Mock<ILocationInventoryService> _mockLocationInventoryService = new();
        private readonly Mock<ISegmentTravelDataService> _mockTravelDataService = new();
        private readonly Mock<ISdk<IEntitiesClient>> _mockEntitiesClient = new Mock<ISdk<IEntitiesClient>>();
        private readonly Mock<ITokenHandler> _mockTokenHandler = new Mock<ITokenHandler>();

        public WorkerTests()
        {
            _mockServiceOptionsCache = new Mock<IServiceOptionsCache>();
            SetupMocks();
        }

        private void SetupMocks()
        {
            _mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_mockLogger.Object);
            _mockConfiguration.Setup(c => c["Tenant:Id"]).Returns("68ae2153-e56f-4bd5-9731-04521062f567");

            var mockMetricsCounter = new Mock<IMetricsCounter>();
            _mockMetricsFactory.Setup(m => m.GetMetricsCounter(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mockMetricsCounter.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldInsertNewDatabaseRecord_WhenCalledAsync()
        {
            var worker = new Worker.Worker(_mockActionEventStatusSink.Object, _mockServiceProvider.Object,
                _mockMetricsFactory.Object, _mockEntitiesClient.Object, _mockTokenHandler.Object,
                _mockConfiguration.Object, _mockLoggerFactory.Object);
            await worker.ExecutePublicAsync(CancellationToken.None);

            await worker.ExecutePublicAsync(CancellationToken.None);


            _mockSegmentInventoryService.Verify(s => s.UpdateAsync(It.IsAny<SegmentInventoryDocument>()),
                Times.AtLeastOnce());
            _mockTravelDataService.Verify(t => t.UpdateAsync(It.IsAny<TravelDataDocument>()), Times.AtLeastOnce());
            _mockLocationInventoryService.Verify(x => x.UpdateAsync(It.IsAny<LocationInventoryDocument>()),
                Times.AtLeastOnce());

            _mockSegmentInventoryService.Invocations.Count.Should().BeGreaterOrEqualTo(1);
            _mockTravelDataService.Invocations.Count.Should().BeGreaterOrEqualTo(1);
            _mockLocationInventoryService.Invocations.Count.Should().BeGreaterOrEqualTo(1);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldUpdateExistingDatabaseRecord_WhenCalledAsync()
        {
            var worker = new Worker.Worker(_mockActionEventStatusSink.Object, _mockServiceProvider.Object,
                _mockMetricsFactory.Object, _mockEntitiesClient.Object, _mockTokenHandler.Object,
                _mockConfiguration.Object, _mockLoggerFactory.Object);
            await worker.ExecutePublicAsync(CancellationToken.None);

            _mockSegmentInventoryService.Verify(s => s.UpdateAsync(It.IsAny<SegmentInventoryDocument>()), Times.Once());
            _mockTravelDataService.Verify(t => t.UpdateAsync(It.IsAny<TravelDataDocument>()), Times.Once());
            _mockLocationInventoryService.Verify(x => x.UpdateAsync(It.IsAny<LocationInventoryDocument>()),
                Times.Once());


            _mockSegmentInventoryService.Invocations.Count.Should().Be(1);
            _mockTravelDataService.Invocations.Count.Should().Be(1);
            _mockLocationInventoryService.Invocations.Count.Should().Be(1);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogAndRethrowOperationCanceledException_WhenCanceledAsync()
        {
            var worker = new Worker.Worker(_mockActionEventStatusSink.Object, _mockServiceProvider.Object,
                _mockMetricsFactory.Object, _mockEntitiesClient.Object, _mockTokenHandler.Object,
                _mockConfiguration.Object, _mockLoggerFactory.Object);
            var cts = new CancellationTokenSource();
            cts.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(() => worker.ExecutePublicAsync(cts.Token));

            _mockLogger.Verify(l => l.LogError(It.IsAny<OperationCanceledException>(), It.IsAny<string>()),
                Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogAndSwallowAnyOtherException_WhenThrownAsync()
        {
            var worker = new Worker.Worker(_mockActionEventStatusSink.Object, _mockServiceProvider.Object,
                _mockMetricsFactory.Object, _mockEntitiesClient.Object, _mockTokenHandler.Object,
                _mockConfiguration.Object, _mockLoggerFactory.Object);

            await worker.ExecutePublicAsync(CancellationToken.None);

            var exception = new Exception("Error Thrown");
            _mockAcyclicaService.Setup(a => a.GetSegmentInventoryAsync()).ThrowsAsync(exception);


            await worker.ExecutePublicAsync(CancellationToken.None);


            _mockLogger.Verify(l => l.LogError(exception, It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_ShouldEndTheMainLoop_WhenFinishedAsync()
        {
            var worker = new Worker.Worker(_mockActionEventStatusSink.Object, _mockServiceProvider.Object,
                _mockMetricsFactory.Object, _mockEntitiesClient.Object, _mockTokenHandler.Object,
                _mockConfiguration.Object, _mockLoggerFactory.Object);

            await worker.ExecutePublicAsync(CancellationToken.None);

            _mockLogger.Verify(l => l.LogInformation("Main loop ended"), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_ShouldHandleValidGeolocationDataAsync()
        {
            var validLocation = new Models.LocationInventory.Location
            {
                Locations = new List<LocationInventory>
                    { new LocationInventory { LocationId = 123, Latitude = 40.7128, Longitude = -74.0060 } }
            };
            _mockAcyclicaService.Setup(s => s.GetLocationInventoryAsync()).ReturnsAsync(validLocation);

            var worker = new Worker.Worker(_mockActionEventStatusSink.Object, _mockServiceProvider.Object,
                _mockMetricsFactory.Object, _mockEntitiesClient.Object, _mockTokenHandler.Object,
                _mockConfiguration.Object, _mockLoggerFactory.Object);

            await worker.ExecutePublicAsync(CancellationToken.None);

            _mockLogger.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ExecuteAsync_ShouldHandleInvalidGeolocationDataAsync()
        {
            var invalidLocation = new Models.LocationInventory.Location
            {
                Locations = new List<LocationInventory>
                    { new LocationInventory { LocationId = 123, Latitude = 200, Longitude = 200 } }
            };
            var exception = new Exception("Error Thrown");
            _mockAcyclicaService.Setup(a => a.GetLocationInventoryAsync()).ThrowsAsync(exception);

            var worker = new Worker.Worker(_mockActionEventStatusSink.Object, _mockServiceProvider.Object,
                _mockMetricsFactory.Object, _mockEntitiesClient.Object, _mockTokenHandler.Object,
                _mockConfiguration.Object, _mockLoggerFactory.Object);

            await worker.ExecutePublicAsync(CancellationToken.None);

            _mockLogger.Verify(l => l.LogError(exception, It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public async Task CreateAndAddEntity_ShouldLogMessage_WhenCalledAsync()
        {
            // Arrange
            var mockEntitiesClient = new Mock<IEntitiesClient>();
            var mockTokenHandler = new Mock<ITokenHandler>();
            var mockLogger = new Mock<ILogger<Worker.Worker>>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
            mockTokenHandler.Setup(x => x.GetTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync("test_token");
            _mockEntitiesClient.Setup(x => x.CreateClient("test_token")).Returns(mockEntitiesClient.Object);
            mockEntitiesClient.Setup(x => x.EntitiesPUTAsync(It.IsAny<EntityNode>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            var validSegmentInventory = new List<SegmentInventory>
{
            new SegmentInventory
    {
            Id = Guid.NewGuid(),
             SegmentId = 1001
    }
};

            var validLocation = new Models.LocationInventory.Location
            {
                Locations = new List<LocationInventory>
            { new LocationInventory { LocationId = 123, Latitude = 40.7128, Longitude = -74.0060 } }
            };
            _mockAcyclicaService.Setup(a => a.GetSegmentInventoryAsync()).Returns(Task.FromResult(validSegmentInventory));
            _mockAcyclicaService.Setup(s => s.GetLocationInventoryAsync()).ReturnsAsync(validLocation);
            // Act
            var worker = new Worker.Worker(_mockActionEventStatusSink.Object, _mockServiceProvider.Object,
                _mockMetricsFactory.Object, _mockEntitiesClient.Object, _mockTokenHandler.Object,
                _mockConfiguration.Object, _mockLoggerFactory.Object);
            await worker.ExecutePublicAsync(CancellationToken.None);


            _mockAcyclicaService.Verify(mock => mock.GetSegmentInventoryAsync(), Times.Once());
            // Assert
            mockLogger.Verify(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => string.Equals("Entity Added", v.ToString(), StringComparison.InvariantCultureIgnoreCase)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public async Task CreateAndAddEntity_ShouldFail_WhenCalledAsync()
        {
            // Arrange
            var mockEntitiesClient = new Mock<IEntitiesClient>();
            var mockTokenHandler = new Mock<ITokenHandler>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();
            mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
            mockTokenHandler.Setup(x => x.GetTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync("test_token");
            _mockEntitiesClient.Setup(x => x.CreateClient("test_token")).Returns(mockEntitiesClient.Object);
            mockEntitiesClient.Setup(x => x.EntitiesPUTAsync(It.IsAny<EntityNode>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

            var validLocation = new Models.LocationInventory.Location
            {
                Locations = new List<LocationInventory>
            { new LocationInventory { LocationId = 123, Latitude = 40.7128, Longitude = -74.0060 } }
            };
            _mockAcyclicaService.Setup(s => s.GetLocationInventoryAsync()).ReturnsAsync(validLocation);

            // Act
            var worker = new Worker.Worker(_mockActionEventStatusSink.Object, _mockServiceProvider.Object,
                _mockMetricsFactory.Object, _mockEntitiesClient.Object, _mockTokenHandler.Object,
                _mockConfiguration.Object, _mockLoggerFactory.Object);

            // Assert
            mockEntitiesClient.Verify(x => x.EntitiesPUTAsync(It.IsAny<EntityNode>(), It.IsAny<CancellationToken>()), Times.Never);
        }

    }
}