using WebAPIN5.Models;

namespace WebAPIN5.Services
{
    public interface IElasticsearchService
    {
        Task IndexPermissionAsync(Permission permission);
    }
}
