
using AcyclicaService.Models.LocationInventory;
using AcyclicaService.Repository;
using AcyclicaService.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace AcyclicaService.Services.Database
{
    public class LocationInventoryService : ILocationInventoryService
    {
        private readonly IAcyclicaLocationInventoryRepository _acyclicalocationInentoryRepository;
        private readonly ILogger<LocationInventoryDocument> _logger;

        public LocationInventoryService(IAcyclicaLocationInventoryRepository acyclicaLocationInventoryRepository, ILogger<LocationInventoryDocument> logger)
        {
            _acyclicalocationInentoryRepository = acyclicaLocationInventoryRepository;
            _logger = logger;

        }

        public async Task<List<LocationInventoryDocument>> GetAllAsync()
        {
            var LocationInventory = await _acyclicalocationInentoryRepository.GetAllAsync();

            return LocationInventory.ToList() ?? new List<LocationInventoryDocument>();
        }

        public async Task<LocationInventoryDocument> GetAsync(Guid id)
        {
            var LocationInventory = await _acyclicalocationInentoryRepository.GetByIdAsync(id);

            return LocationInventory ?? new LocationInventoryDocument();
        }

        public async Task<LocationInventoryDocument> CreateAsync(LocationInventoryDocument locationInventoryDocument)
        {
            try
            {
                var locationInventory = await _acyclicalocationInentoryRepository.GetByIdAsync(locationInventoryDocument.Id);
                if (locationInventory == null)
                {
                    _acyclicalocationInentoryRepository.Add(locationInventoryDocument);
                }
                var (success, _) = await _acyclicalocationInentoryRepository.DbContext.SaveChangesAsync();
                return locationInventoryDocument;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }

        public async Task UpdateAsync(LocationInventoryDocument LocationInventoryDocument)
        {
            var locationInventory = await _acyclicalocationInentoryRepository.GetByIdAsync(LocationInventoryDocument.Id);

            if (locationInventory == null)
            {
                throw new Exception("LocationInventoryDocument not found");
            }
            _acyclicalocationInentoryRepository.Update(locationInventory);

            var (success, errors) = await _acyclicalocationInentoryRepository.DbContext.SaveChangesAsync();
            if (!success && !string.IsNullOrWhiteSpace(errors)) throw new Exception(errors);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                _acyclicalocationInentoryRepository.Remove(id);
                var (success, errors) = await _acyclicalocationInentoryRepository.DbContext.SaveChangesAsync();
                return success;
            }
            catch (Exception e)
            {

                _logger.LogError(e.Message);
                return false;
            }
        }
    }
}
