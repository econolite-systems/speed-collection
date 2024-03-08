using System;
using System.Net.Http;
using Microsoft.Extensions.Options;

namespace EntityService
{
    public partial interface ISdkClient
    {
        string BaseUrl { get; set; }
        string Token { get; set; }
    }

    public interface ISdk<out TClient> where TClient : ISdkClient
    {
        TClient CreateClient(string token);
    }

    public class Sdk<TClient> : ISdk<TClient> where TClient : ISdkClient
    {
        private TClient _client;

        public Sdk(IHttpClientFactory factory, Func<HttpClient, TClient> clientFactory, IOptions<EntityOptions> options)
        {
            _client = clientFactory(factory.CreateClient(options.Value.HttpClientName));
            _client.BaseUrl = options.Value.BaseUrl;
        }

        public TClient CreateClient(string token)
        {
            _client.Token = token;
            return _client;
        }

        public TClient CreateClient(Func<string> token)
        {
            return this.CreateClient(token());
        }
    }
}
