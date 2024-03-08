using AcyclicaConfigRepository.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace AcyclicaConfigRepository.Extensions.Acyclica
{
    /// <summary>
    /// AcyclicaConfigRepositoryExtensions
    /// </summary>
    public static class AcyclicaConfigRepositoryExtensions
    {
        /// <summary>
        /// Add the AcyclicaConfigRepository service
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAcyclicaConfigRepository(this IServiceCollection services)
        {
            services.AddScoped<IAcyclicaConfigRepository, AcyclicaConfigRepository.Repositories.AcyclicaConfigRepository>();
            return services;
        }
    }

}
