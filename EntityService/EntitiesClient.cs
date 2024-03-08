using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityService
{
    public partial interface IEntitiesClient : ISdkClient
    {
    }

    public partial class EntitiesClient
    {
        public string Token { get; set; }

        partial void PrepareRequest(HttpClient client,
            HttpRequestMessage request, string url)
        {
            if (!request.Headers.TryGetValues("Authorization", out var values))
            {
                request.Headers.Add("Authorization", $"Bearer {Token}");
            }
        }

        partial void PrepareRequest(HttpClient client,
            HttpRequestMessage request, StringBuilder urlBuilder)
        {
            if (!request.Headers.TryGetValues("Authorization", out var values))
            {
                request.Headers.Add("Authorization", $"Bearer {Token}");
            }
        }
    }
}
