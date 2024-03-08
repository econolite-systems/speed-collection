
using SegmentCollection.Models;

namespace SegmentCollection.Services.Database
{
    public interface ISegmentCollectionSegmentService
    {
        Task<List<SegmentCollectionSegmentDocument>> GetAllAsync();
        Task<SegmentCollectionSegmentDocument> GetAsync(Guid id);
        Task<SegmentCollectionSegmentDocument> CreateAsync(SegmentCollectionSegmentDocument LongQueueSegmentDocument);
        Task UpdateAsync(SegmentCollectionSegmentDocument LongQueueSegmentDocument);
        Task<bool> DeleteAsync(Guid id);
    }
}
