using AcyclicaService.Repository;
using AcyclicaService.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace AcyclicaService.Services.Database
{
    public class TravelDataService : ISegmentTravelDataService
    {
        private readonly IAcyclicaTravelDataRepository _acyclicaTravelDataRepository;
        private readonly ILogger<TravelDataDocument> _logger;

        public TravelDataService(IAcyclicaTravelDataRepository acyclicaTravelDataRepository, ILogger<TravelDataDocument> logger)
        {
            _acyclicaTravelDataRepository = acyclicaTravelDataRepository;
            _logger = logger;

        }

        public async Task<List<TravelDataDocument>> GetAllForSegmentAsync(int segmentId)
        {
            var TravelData = await _acyclicaTravelDataRepository.GetAllAsync();

            return TravelData.Where(x => x.SegmentId == segmentId).ToList() ?? new List<TravelDataDocument>();
        }
        public async Task<List<TravelDataDocument>> GetAllAsync()
        {
            var TravelData = await _acyclicaTravelDataRepository.GetAllAsync();

            return TravelData.ToList() ?? new List<TravelDataDocument>();
        }

        public async Task<TravelDataDocument> GetAsync(Guid id)
        {
            var TravelData = await _acyclicaTravelDataRepository.GetByIdAsync(id);

            return TravelData ?? new TravelDataDocument();
        }

        public async Task<TravelDataDocument> CreateAsync(TravelDataDocument TravelDataDocument)
        {
            try
            {
                var TravelData = await _acyclicaTravelDataRepository.GetByIdAsync(TravelDataDocument.Id);
                if (TravelData == null)
                {
                    _acyclicaTravelDataRepository.Add(TravelDataDocument);
                }
                var (success, _) = await _acyclicaTravelDataRepository.DbContext.SaveChangesAsync();
                return TravelDataDocument;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }

        public async Task UpdateAsync(TravelDataDocument TravelDataDocument)
        {
            var TravelData = await _acyclicaTravelDataRepository.GetByIdAsync(TravelDataDocument.Id);

            if (TravelData == null)
            {
                throw new Exception("TravelDataDocument not found");
            }
            _acyclicaTravelDataRepository.Update(TravelData);

            var (success, errors) = await _acyclicaTravelDataRepository.DbContext.SaveChangesAsync();
            if (!success && !string.IsNullOrWhiteSpace(errors)) throw new Exception(errors);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                _acyclicaTravelDataRepository.Remove(id);
                var (success, errors) = await _acyclicaTravelDataRepository.DbContext.SaveChangesAsync();
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
