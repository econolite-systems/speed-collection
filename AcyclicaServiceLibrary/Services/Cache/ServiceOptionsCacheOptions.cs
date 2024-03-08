namespace AcyclicaService.Services.Cache
{
    public class ServiceOptionsCacheOptions
    {
        public TimeSpan CacheRefreshPeriod { get; set; } = TimeSpan.FromMinutes(15);
    }
}
