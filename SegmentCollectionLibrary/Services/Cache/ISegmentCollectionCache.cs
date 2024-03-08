using SegmentCollection.Models;

namespace SegmentCollection.Services.Cache
{
    public interface ISegmentCollectionCache
    {
        Task UpdateSegmentCollectionCacheAsync(List<SegmentCollectionSegment> segments);
        Task GetSegmentCollectionCacheAsync();
    }
}