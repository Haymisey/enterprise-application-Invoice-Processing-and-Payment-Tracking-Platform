using MediatR;
using Shared.Domain.Results;

namespace Shared.Application.Messaging;

/// <summary>
/// Marker interface for commands that don't return a value.
/// Commands represent intentions to change the system state.
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// Command that returns a value of type TResponse.
/// </summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
