using AcyclicaService.Models.LocationInventory;
using AcyclicaService.Models.Segments;
using AcyclicaService.Models.TravelTime;

namespace AcyclicaService.Services.Api
{
    public interface IAcyclicaApiService
    {
        Task<List<SegmentInventory>> GetSegmentInventoryAsync();
        Task<List<TravelData>> GetSegmentTravelDataAsync(string format, int segmentId, long startTimeEpoch, long endTimeEpoch, int period);
        Task<Location> GetLocationInventoryAsync();

    }
}