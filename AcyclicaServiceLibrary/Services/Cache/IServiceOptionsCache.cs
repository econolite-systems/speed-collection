namespace AcyclicaService.Services.Cache
{
    public interface IServiceOptionsCache
    {
        public Task<ServiceOptions.ServiceOptions> GetOptionsAsync();
        public bool IsRefreshNeeded();
    }
}
