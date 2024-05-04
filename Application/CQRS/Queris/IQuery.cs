using MediatR;

namespace Application.Cqrs.Queris;

public interface IQuery<TResult> : IRequest<TResult>
{
}
