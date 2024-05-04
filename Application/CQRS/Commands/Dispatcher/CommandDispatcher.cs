using Application.Cqrs.Commands;
using MediatR;

namespace Application.Cqrs.Commands.Dispatcher;
public class CommandDispatcher : ICommandDispatcher
{
    private readonly IMediator mediator;

    public CommandDispatcher(IMediator mediator)
    {
        this.mediator = mediator;
    }

    public Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        return mediator.Send(command, cancellationToken);
    }
}