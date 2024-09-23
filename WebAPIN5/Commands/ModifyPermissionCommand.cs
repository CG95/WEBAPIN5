using WebAPIN5.Models;
using WebAPIN5.Repositories;

namespace WebAPIN5.Commands
{
    public class ModifyPermissionCommand : IRequest<Permission>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ModifyPermissionCommand(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task HandleAsync(Permission permission)
        {
            _unitOfWork.PermissionRepository.Update(permission);
            await _unitOfWork.CommitAsync();
        }
    }
}
