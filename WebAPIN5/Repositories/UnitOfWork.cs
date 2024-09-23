using WebAPIN5.Data;
using WebAPIN5.Models;

namespace WebAPIN5.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IRepository<Permission> PermissionRepository { get; }
        public IRepository<PermissionType> PermissionTypeRepository { get; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            PermissionRepository = new Repository<Permission>(context);
            PermissionTypeRepository = new Repository<PermissionType>(context);
        }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
