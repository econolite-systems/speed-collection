using System.Text.Json;
using AcyclicaService.Models.LocationInventory;
using AcyclicaService.Models.Segments;
using AcyclicaService.Models.TravelTime;
using AcyclicaService.Services.Cache;

namespace AcyclicaService.Services.Api
{
    public class AcyclicaApiService : IAcyclicaApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IServiceOptionsCache _serviceOptionsCache;
        public AcyclicaApiService(HttpClient httpClient, IServiceOptionsCache serviceOptionsCache)
        {
            _httpClient = httpClient;
            _serviceOptionsCache = serviceOptionsCache;
        }

        public async Task<List<SegmentInventory>> GetSegmentInventoryAsync()
        {
            var serviceOptions = await _serviceOptionsCache.GetOptionsAsync();
            var apiKey = serviceOptions.ApiKey;
            var uri = $"{serviceOptions.BaseUrl}/datastream/segment/inventory/json";
            _httpClient.DefaultRequestHeaders.Add("Authentication", $"Token {apiKey}");
            var response = await _httpClient.GetAsync(uri);
            if (response.IsSuccessStatusCode && response?.Content != null)
            {
                var json = await response.Content.ReadAsStringAsync();
                _httpClient.DefaultRequestHeaders.Clear();
                return JsonSerializer.Deserialize<List<SegmentInventory>>(json) ?? new List<SegmentInventory>();
            }
            else if (response?.Content == null)
            {
                throw new Exception("The response was null.");
            }
            else
            {
                throw new Exception($"The HTTP status code of the response was not expected, {response.StatusCode} with url {uri}\n with API Key {serviceOptions.ApiKey}, and Polling Rate {serviceOptions.PollingRate}");
            }
        }

        public async Task<Location> GetLocationInventoryAsync()
        {
            var serviceOptions = await _serviceOptionsCache.GetOptionsAsync();
            var apiKey = serviceOptions.ApiKey;
            var uri = $"{serviceOptions.BaseUrl}/datastream/location/inventory/json";
            _httpClient.DefaultRequestHeaders.Add("Authentication", $"Token {apiKey}");
            var response = await _httpClient.GetAsync(uri);
            if (response.IsSuccessStatusCode && response?.Content != null)
            {
                var json = await response.Content.ReadAsStringAsync();
                _httpClient.DefaultRequestHeaders.Clear();
                return JsonSerializer.Deserialize<Location>(json) ?? new Location();
            }
            else if (response?.Content == null)
            {
                throw new Exception("The response was null.");
            }
            else
            {
                throw new Exception($"The HTTP status code of the response was not expected, {response.StatusCode} with url {uri}");
            }
        }

        public async Task<List<TravelData>> GetSegmentTravelDataAsync(string format, int segmentId, long startTimeEpoch, long endTimeEpoch, int period = 60000)
        {
            var serviceOptions = await _serviceOptionsCache.GetOptionsAsync();
            var apiKey = serviceOptions.ApiKey;
            var uri = $"{serviceOptions.BaseUrl}/datastream/segment/travel_time/{format}/{segmentId}/{startTimeEpoch}/{endTimeEpoch}/{period}";
            _httpClient.DefaultRequestHeaders.Add("Authentication", $"Token {apiKey}");
            var response = await _httpClient.GetAsync(uri);
            if (response.IsSuccessStatusCode && response?.Content != null)
            {
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<List<List<long>>>(json) ?? new List<List<long>>();

                var travelDataList = new List<TravelData>();

                // Create new TravelData objects from the lists
                foreach (var list in data)
                {
                    var travelData = new TravelData
                    {
                        SegmentId = segmentId,
                        Time = list[0],
                        Strength = (int)list[1],
                        First = (int)list[2],
                        Last = (int)list[3],
                        Minimum = (int)list[4],
                        Maximum = (int)list[5]
                    };

                    travelDataList.Add(travelData);
                }
                _httpClient.DefaultRequestHeaders.Clear();
                return travelDataList;
            }
            else if (response?.Content == null)
            {
                throw new Exception("The response was null.");
            }
            else
            {
                throw new Exception($"The HTTP status code of the response was not expected, {response.StatusCode} with url {uri}");
            }
        }
    }
}
