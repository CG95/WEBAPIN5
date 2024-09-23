using WebAPIN5.Models;
using WebAPIN5.Repositories;

namespace WebAPIN5.Queries
{
    public class GetAllPermissionsQuery : IQuery<IEnumerable<Permission>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllPermissionsQuery(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Permission>> ExecuteAsync()
        {
            return await _unitOfWork.PermissionRepository.GetAllAsync();
        }
    }
}
