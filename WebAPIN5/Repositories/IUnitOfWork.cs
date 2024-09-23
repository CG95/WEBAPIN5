using WebAPIN5.Models;

namespace WebAPIN5.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Permission> PermissionRepository { get; }
        IRepository<PermissionType> PermissionTypeRepository { get; }
        Task<int> CommitAsync();
    }
}
