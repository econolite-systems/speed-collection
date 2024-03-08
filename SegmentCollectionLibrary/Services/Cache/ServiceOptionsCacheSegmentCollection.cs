using AcyclicaConfigRepository.Repositories;
using Microsoft.Extensions.Options;

namespace SegmentCollection.Services.Cache
{
    public class ServiceOptionsCacheSegmentCollection : IServiceOptionsSegmentCollectionCache
    {
        private readonly IAcyclicaConfigRepository _acyclicaConfigRepository;
        private readonly SegmentCollectionCacheOptions _options;
        private ServiceOptionsSegmentCollection? _serviceOptions = new();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private DateTime _lastFetchTime = DateTime.MinValue;

        public ServiceOptionsCacheSegmentCollection(IAcyclicaConfigRepository acyclicaConfigRepository, IOptions<SegmentCollectionCacheOptions> options)
        {
            _acyclicaConfigRepository = acyclicaConfigRepository;
            _options = options?.Value ?? new SegmentCollectionCacheOptions();
        }

        public async Task<ServiceOptionsSegmentCollection> GetOptionsAsync()
        {
            if (DateTime.UtcNow - _lastFetchTime > _options.CacheRefreshPeriod)
            {
                try
                {
                    await _semaphore.WaitAsync();
                    if (DateTime.UtcNow - _lastFetchTime > _options.CacheRefreshPeriod)
                    {
                        var configs = await _acyclicaConfigRepository.FindOneAsync();
                        if (configs != null)
                        {
                            _serviceOptions = new ServiceOptionsSegmentCollection()
                            {
                                PollingRate = configs.PollInterval
                            };
                            _lastFetchTime = DateTime.UtcNow;
                        }
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            return _serviceOptions ?? new ServiceOptionsSegmentCollection();
        }

    }
}