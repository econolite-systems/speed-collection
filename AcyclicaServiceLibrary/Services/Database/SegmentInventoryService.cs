using AcyclicaService.Repository;
using AcyclicaService.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace AcyclicaService.Services.Database
{
    public class SegmentInventoryService : ISegmentInventoryService
    {
        private readonly IAcyclicaSegmentInventoryRepository _acyclicaSegmentInentoryRepository;
        private readonly ILogger<SegmentInventoryDocument> _logger;

        public SegmentInventoryService(IAcyclicaSegmentInventoryRepository acyclicaSegmentInventoryRepository, ILogger<SegmentInventoryDocument> logger)
        {
            _acyclicaSegmentInentoryRepository = acyclicaSegmentInventoryRepository;
            _logger = logger;

        }

        public async Task<List<SegmentInventoryDocument>> GetAllAsync()
        {
            var SegmentInventory = await _acyclicaSegmentInentoryRepository.GetAllAsync();

            return SegmentInventory.ToList() ?? new List<SegmentInventoryDocument>();
        }

        public async Task<SegmentInventoryDocument> GetAsync(Guid id)
        {
            var SegmentInventory = await _acyclicaSegmentInentoryRepository.GetByIdAsync(id);

            return SegmentInventory ?? new SegmentInventoryDocument();
        }

        public async Task<SegmentInventoryDocument> CreateAsync(SegmentInventoryDocument SegmentInventoryDocument)
        {
            try
            {
                var SegmentInventory = await _acyclicaSegmentInentoryRepository.GetByIdAsync(SegmentInventoryDocument.Id);
                if (SegmentInventory == null)
                {
                    _acyclicaSegmentInentoryRepository.Add(SegmentInventoryDocument);
                }
                var (success, _) = await _acyclicaSegmentInentoryRepository.DbContext.SaveChangesAsync();
                return SegmentInventoryDocument;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }

        public async Task UpdateAsync(SegmentInventoryDocument SegmentInventoryDocument)
        {
            var SegmentInventory = await _acyclicaSegmentInentoryRepository.GetByIdAsync(SegmentInventoryDocument.Id);

            if (SegmentInventory == null)
            {
                throw new Exception("SegmentInventoryDocument not found");
            }
            _acyclicaSegmentInentoryRepository.Update(SegmentInventory);

            var (success, errors) = await _acyclicaSegmentInentoryRepository.DbContext.SaveChangesAsync();
            if (!success && !string.IsNullOrWhiteSpace(errors)) throw new Exception(errors);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                _acyclicaSegmentInentoryRepository.Remove(id);
                var (success, errors) = await _acyclicaSegmentInentoryRepository.DbContext.SaveChangesAsync();
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