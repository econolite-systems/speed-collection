using AcyclicaService.Repository;

namespace AcyclicaService.Services.Database
{
    public interface ISegmentTravelDataService
    {
        Task<List<TravelDataDocument>> GetAllForSegmentAsync(int segmentId);
        Task<List<TravelDataDocument>> GetAllAsync();
        Task<TravelDataDocument> GetAsync(Guid id);
        Task<TravelDataDocument> CreateAsync(TravelDataDocument TravelDataDocument);
        Task UpdateAsync(TravelDataDocument TravelDataDocument);
        Task<bool> DeleteAsync(Guid id);
    }
}