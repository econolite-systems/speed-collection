using AcyclicaService.Repository;

namespace AcyclicaService.Services.Database
{
    public interface ISegmentInventoryService
    {
        Task<List<SegmentInventoryDocument>> GetAllAsync();
        Task<SegmentInventoryDocument> GetAsync(Guid id);
        Task<SegmentInventoryDocument> CreateAsync(SegmentInventoryDocument SegmentInventoryDocument);
        Task UpdateAsync(SegmentInventoryDocument SegmentInventoryDocument);
        Task<bool> DeleteAsync(Guid id);
    }
}