using System.Net;
using System.Text;
using System.Text.Json;
using AcyclicaService.Models.LocationInventory;
using AcyclicaService.Models.Segments;
using AcyclicaService.Services.Api;
using AcyclicaService.Services.Cache;
using RichardSzalay.MockHttp;

namespace AcyclicaService.Test
{
    public class AcyclicaServiceTests
    {

        private readonly string baseApiEndpoint = "https://go.acyclica.com/";
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly Mock<IServiceOptionsCache> _serviceOptionsCacheMock;
        private readonly AcyclicaApiService _acyclicaService;

        public AcyclicaServiceTests()
        {
            _mockHttp = new MockHttpMessageHandler();
            _serviceOptionsCacheMock = new Mock<IServiceOptionsCache>();
            _acyclicaService = new AcyclicaApiService(new HttpClient(_mockHttp), _serviceOptionsCacheMock.Object);
        }

        [Fact]
        public async Task TestGetSegmentInventoryGetsAppropriateDataAndShouldReturnsValidDataAsync()
        {
            var segmentInventory = new SegmentInventory
            {
                Id = Guid.NewGuid(),
                SegmentId = 1,
                StartSerial = 123,
                StartId = 1,
                EndSerial = 456,
                EndId = 2,
                Distance = 1.0,
                Polyline = "somePolyline",
                BaseOffset = 1,
                StartLocationId = 1,
                StartLocationName = "someStartLocationName",
                EndLocationName = "someEndLocationName",
                EndLocationId = 2
            };

            var segmentInventoryList = new List<SegmentInventory> { segmentInventory };

            _serviceOptionsCacheMock.Setup(s => s.GetOptionsAsync())
                .ReturnsAsync(new ServiceOptions.ServiceOptions
                {
                    PollingRate = 60,
                    ApiKey = "testApiKey",
                    BaseUrl = baseApiEndpoint
                });

            string url = baseApiEndpoint + "/datastream/segment/inventory/json";
            _mockHttp.When(HttpMethod.Get, url)
                .Respond(HttpStatusCode.OK, "application/json", JsonSerializer.Serialize(segmentInventoryList));

            var test = await _acyclicaService.GetSegmentInventoryAsync();
            test.Should().NotBeNull();
            test.Should().BeEquivalentTo(segmentInventoryList);
        }


        [Fact]
        public async Task TestGetSegmentInventoryFailsAndGetsNullDataAsync()
        {
            _serviceOptionsCacheMock.Setup(s => s.GetOptionsAsync())
                .ReturnsAsync(new ServiceOptions.ServiceOptions
                {
                    PollingRate = 60,
                    ApiKey = "testApiKey",
                    BaseUrl = baseApiEndpoint
                });

            string url = baseApiEndpoint + "/datastream/segment/inventory/json";
            _mockHttp.When(HttpMethod.Get, url)
                .Respond(HttpStatusCode.OK, "application/json", "[]");

            var result = await _acyclicaService.GetSegmentInventoryAsync();

            Func<Task> act = async () => await _acyclicaService.GetSegmentInventoryAsync();

            await act.Should().NotThrowAsync<JsonException>();
        }






        [Fact]
        public async Task TestGetLocationInventoryGetsLocationDataAndShouldReturnValidDataAsync()
        {


            var locationInventory = new LocationInventory
            {
                LocationId = 1001,
                Latitude = 40.7128,
                Longitude = 74.0060,
                DiffrfSensors = new List<int> { 1, 2 },
                Cabinets = new List<int> { 3, 4 },
                VsoSensors = new List<int> { 5, 6 },
                UserFiles = new List<int> { 7, 8 }
            };


            _serviceOptionsCacheMock.Setup(s => s.GetOptionsAsync())
                .ReturnsAsync(new ServiceOptions.ServiceOptions
                {
                    PollingRate = 60,
                    ApiKey = "testApiKey",
                    BaseUrl = baseApiEndpoint
                });


            string url = baseApiEndpoint + "/datastream/location/inventory/json";
            _mockHttp.When(HttpMethod.Get, url)
           .Respond(HttpStatusCode.OK, "application/json", JsonSerializer.Serialize(locationInventory));

            var test = await _acyclicaService.GetLocationInventoryAsync();
            test.Should().NotBeNull();
        }

        [Fact]
        public async Task TestGetLocationInventoryReturnsNewLocationWhenReceivesEmptyJsonObjectAsync()
        {
            _serviceOptionsCacheMock.Setup(s => s.GetOptionsAsync())
                .ReturnsAsync(new ServiceOptions.ServiceOptions
                {
                    PollingRate = 60,
                    ApiKey = "testApiKey",
                    BaseUrl = baseApiEndpoint
                });

            string url = baseApiEndpoint + "/datastream/location/inventory/json";
            _mockHttp.When(HttpMethod.Get, url)
                .Respond(HttpStatusCode.OK, "application/json", "{}");

            var result = await _acyclicaService.GetLocationInventoryAsync();

            result.Should().BeEquivalentTo(new Location());
        }





        [Fact]
        public async Task TestGetTravelTimeDataShouldReturnValidDataAsync()
        {
            var travelDataList = new List<List<long>>
    {
        new List<long> { 1551445200000, 15606, 15464, 15051, 14490, 16094 },
        new List<long> { 1551445500000, 15393, 15393, 15215, 14573, 15822 }
    };

            _serviceOptionsCacheMock.Setup(s => s.GetOptionsAsync())
                .ReturnsAsync(new ServiceOptions.ServiceOptions
                {
                    PollingRate = 60,
                    ApiKey = "testApiKey",
                    BaseUrl = baseApiEndpoint
                });

            string url = baseApiEndpoint + "/datastream/segment/travel_time/json/1/1551445200000/1551445500000/60000";
            _mockHttp.When(HttpMethod.Get, url)
           .Respond(HttpStatusCode.OK, "application/json", JsonSerializer.Serialize(travelDataList));

            var test = await _acyclicaService.GetSegmentTravelDataAsync("json", 1, 1551445200000, 1551445500000, 60000);
            test.Should().NotBeNull();
        }



        [Fact]
        public async Task TestGetTravelTimeDataShouldExpectNoDataAndFailAsync()
        {
            _serviceOptionsCacheMock.Setup(s => s.GetOptionsAsync())
                .ReturnsAsync(new ServiceOptions.ServiceOptions
                {
                    PollingRate = 60,
                    ApiKey = "testApiKey",
                    BaseUrl = baseApiEndpoint
                });

            string url = baseApiEndpoint + "/datastream/segment/travel_time/json/1/1551445200000/1551445500000/60000";
            _mockHttp.When(HttpMethod.Get, url)
           .Respond(HttpStatusCode.OK, "application/json", "[]");


            var result = await _acyclicaService.GetSegmentTravelDataAsync("json", 1, 1551445200000, 1551445500000, 60000);

            Func<Task> act = async () => await _acyclicaService.GetSegmentTravelDataAsync("json", 1, 1551445200000, 1551445500000, 60000);

            await act.Should().NotThrowAsync<JsonException>();
        }
    }
}