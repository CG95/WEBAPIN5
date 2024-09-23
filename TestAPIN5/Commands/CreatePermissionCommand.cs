using WebAPIN5.Models;
using WebAPIN5.Repositories;

namespace WebAPIN5.Commands
{
    public class CreatePermissionCommand : IRequest<Permission>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreatePermissionCommand(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task HandleAsync(Permission entity)
        {
            await _unitOfWork.PermissionRepository.AddAsync(entity);
            await _unitOfWork.CommitAsync();
        }
    }
}
