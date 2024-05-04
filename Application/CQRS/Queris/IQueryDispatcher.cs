namespace Application.Cqrs.Queris;

public interface IQueryDispatcher
{
    //Task<TResult> SendAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default) where TQuery : class, IQuery<TResult>;
    Task<TResult> SendAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);/* where TQuery : class, IQuery<TResult>;*/
}
