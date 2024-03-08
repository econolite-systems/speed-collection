using AcyclicaConfigRepository.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AcyclicaConfigRepository.Extensions.Acyclica
{
    /// <summary>
    /// AcyclicaConfigServiceExtensions
    /// </summary>
    public static class AcyclicaConfigServiceExtensions
    {
        /// <summary>
        /// Add the AcyclicaConfigService service
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAcyclicaConfigService(this IServiceCollection services)
        {
            services.AddScoped<IAcyclicaConfigService, AcyclicaConfigService>();
            return services;
        }
    }

}
