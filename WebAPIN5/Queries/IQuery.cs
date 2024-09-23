namespace WebAPIN5.Queries
{
    public interface IQuery<TResult>
    {
        Task<TResult> ExecuteAsync();
    }
}
