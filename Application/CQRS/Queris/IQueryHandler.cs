using MediatR;

namespace Application.Cqrs.Queris;

public interface IQueryHandler<TQuery, TResult> : IRequestHandler<TQuery, TResult> where TQuery : class, IQuery<TResult>
{
}
