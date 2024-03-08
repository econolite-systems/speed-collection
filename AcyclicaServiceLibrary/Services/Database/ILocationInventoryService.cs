using AcyclicaService.Repository;

namespace AcyclicaService.Services.Database
{
    public interface ILocationInventoryService
    {
        Task<List<LocationInventoryDocument>> GetAllAsync();
        Task<LocationInventoryDocument> GetAsync(Guid id);
        Task<LocationInventoryDocument> CreateAsync(LocationInventoryDocument locationInventoryDocument);
        Task UpdateAsync(LocationInventoryDocument LocationInventoryDocument);
        Task<bool> DeleteAsync(Guid id);
    }
}