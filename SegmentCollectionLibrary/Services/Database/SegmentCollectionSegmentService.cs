using Microsoft.Extensions.Logging;
using SegmentCollection.Models;
using SegmentCollection.Repository.Interfaces;

namespace SegmentCollection.Services.Database
{
    public class SegmentCollectionSegmentService : ISegmentCollectionSegmentService
    {
        private readonly ISegmentCollectionRepository _segmentCollectionRepository;
        private readonly ILogger<SegmentCollectionSegmentDocument> _logger;

        public SegmentCollectionSegmentService(ISegmentCollectionRepository segmentCollectionRepository, ILogger<SegmentCollectionSegmentDocument> logger)
        {
            _segmentCollectionRepository = segmentCollectionRepository;
            _logger = logger;

        }
        public async Task<SegmentCollectionSegmentDocument> CreateAsync(SegmentCollectionSegmentDocument LongQueueSegmentDocument)
        {
            try
            {
                var LongQueueSegment = await _segmentCollectionRepository.GetByIdAsync(LongQueueSegmentDocument.Id);
                if (LongQueueSegment == null)
                {
                    _segmentCollectionRepository.Add(LongQueueSegmentDocument);
                }
                var (success, _) = await _segmentCollectionRepository.DbContext.SaveChangesAsync();
                return LongQueueSegmentDocument;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                _segmentCollectionRepository.Remove(id);
                var (success, errors) = await _segmentCollectionRepository.DbContext.SaveChangesAsync();
                return success;
            }
            catch (Exception e)
            {

                _logger.LogError(e.Message);
                return false;
            }
        }

        public async Task<List<SegmentCollectionSegmentDocument>> GetAllAsync()
        {
            var LongQueueSegment = await _segmentCollectionRepository.GetAllAsync();

            return LongQueueSegment.ToList() ?? new List<SegmentCollectionSegmentDocument>();
        }

        public async Task<SegmentCollectionSegmentDocument> GetAsync(Guid id)
        {
            var LongQueueSegment = await _segmentCollectionRepository.GetByIdAsync(id);

            return LongQueueSegment ?? new SegmentCollectionSegmentDocument();
        }

        public async Task UpdateAsync(SegmentCollectionSegmentDocument LongQueueSegmentDocument)
        {
            var LongQueueSegment = await _segmentCollectionRepository.GetByIdAsync(LongQueueSegmentDocument.Id);

            if (LongQueueSegment == null)
            {
                throw new Exception("LongQueueSegmentDocument not found");
            }
            _segmentCollectionRepository.Update(LongQueueSegment);

            var (success, errors) = await _segmentCollectionRepository.DbContext.SaveChangesAsync();
            if (!success && !string.IsNullOrWhiteSpace(errors)) throw new Exception(errors);
        }
    }
}
