using MediatR;

namespace Shared.Application.Messaging;

/// <summary>
/// Query interface for CQRS pattern.
/// Queries are read-only operations that don't change state.
/// </summary>
public interface IQuery<TResponse> : IRequest<TResponse>
{
}
