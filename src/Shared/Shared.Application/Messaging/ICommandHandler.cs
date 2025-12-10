using MediatR;
using Shared.Domain.Results;

namespace Shared.Application.Messaging;

/// <summary>
/// Command handler for commands that don't return a value.
/// </summary>
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

/// <summary>
/// Command handler for commands that return a value.
/// </summary>
public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
