using Econolite.Ode.Extensions.Sdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace EntityService.ServiceOptions
{
    public static class Defined
    {
        private const string EntitiesHttpClient = "EntitiesHttpClient";
        private const string EntitiesServiceBaseUrl = "EntitiesServiceBaseUrl";

        public static IServiceCollection AddEntitiesConfigServiceCall(this IServiceCollection services, IConfiguration configuration) =>
              services.AddEntitiesConfigServiceCall(options =>
              {
                  options.HttpClientName = EntitiesHttpClient;
                  options.BaseUrl = configuration["Services:Configuration"]!;
              });


        public static IServiceCollection AddEntitiesConfigServiceCall(this IServiceCollection services, Action<EntityOptions> options)
        {
            services.Configure(options);
            services.AddRetryHttpClient(EntitiesHttpClient);
            services.AddSingleton<Func<HttpClient, IEntitiesClient>>((client) =>

                new EntitiesClient(EntitiesServiceBaseUrl, client)
            );
            services.AddSingleton<ISdk<IEntitiesClient>, Sdk<IEntitiesClient>>();
            return services;
        }

    }
}
