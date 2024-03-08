namespace AcyclicaService.ServiceOptions

{
    public class ServiceOptions
    {
        public int PollingRate { get; init; } = default!;
        public string ApiKey { get; init; } = default!;
        public string BaseUrl { get; init; } = default!;
    }
}