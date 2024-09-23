using Nest;
using WebAPIN5.Models;

namespace WebAPIN5.Services
{
    public class ElasticsearchService : IElasticsearchService
    {
        private readonly IElasticClient _client;

        public ElasticsearchService(IElasticClient client)
        {
            _client = client;
        }

        public async Task IndexPermissionAsync(Permission permission)
        {
            var response = await _client.IndexDocumentAsync(permission);
            if (!response.IsValid)
            {
                throw new Exception("Failed to index document into Elasticsearch:\n\n" + response.ServerError);
            }
        }
    }

}
