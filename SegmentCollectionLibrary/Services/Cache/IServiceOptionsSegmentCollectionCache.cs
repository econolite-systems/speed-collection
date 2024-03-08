namespace SegmentCollection.Services.Cache
{
    public interface IServiceOptionsSegmentCollectionCache
    {
        public Task<ServiceOptionsSegmentCollection> GetOptionsAsync();
    }
}