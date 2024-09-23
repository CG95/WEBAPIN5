using WebAPIN5.Models;

namespace WebAPIN5.Commands
{
    public interface IRequest<T> where T : class
    {
        Task HandleAsync(T entity);
    }
}
