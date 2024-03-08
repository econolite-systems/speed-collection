using AcyclicaConfigRepository.Repositories;
using Microsoft.Extensions.Options;

namespace AcyclicaService.Services.Cache
{
    public class ServiceOptionsCache : IServiceOptionsCache
    {
        private readonly IAcyclicaConfigRepository _acyclicaConfigRepository;
        private readonly ServiceOptionsCacheOptions _options;
        private ServiceOptions.ServiceOptions? _serviceOptions = new();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private DateTime _lastFetchTime = DateTime.MinValue;

        public ServiceOptionsCache(IAcyclicaConfigRepository acyclicaConfigRepository, IOptions<ServiceOptionsCacheOptions> options)
        {
            _acyclicaConfigRepository = acyclicaConfigRepository;
            _options = options?.Value ?? new ServiceOptionsCacheOptions();
        }

        public async Task<ServiceOptions.ServiceOptions> GetOptionsAsync()
        {
            if (IsRefreshNeeded())
            {
                try
                {
                    await _semaphore.WaitAsync();
                    if (DateTime.UtcNow - _lastFetchTime > _options.CacheRefreshPeriod)
                    {
                        var configs = await _acyclicaConfigRepository.FindOneAsync();
                        if (configs != null)
                        {
                            _serviceOptions = new ServiceOptions.ServiceOptions()
                            {
                                ApiKey = configs.ApiKey,
                                BaseUrl = configs.Url,
                                PollingRate = configs.PollInterval > 60 ? configs.PollInterval : 60
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
            return _serviceOptions ?? new ServiceOptions.ServiceOptions();
        }
        public bool IsRefreshNeeded() => (DateTime.UtcNow - _lastFetchTime) > _options.CacheRefreshPeriod;
    }
}
