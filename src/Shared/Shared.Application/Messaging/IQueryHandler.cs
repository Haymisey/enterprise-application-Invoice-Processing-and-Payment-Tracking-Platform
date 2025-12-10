using MediatR;

namespace Shared.Application.Messaging;

/// <summary>
/// Query handler interface for CQRS pattern.
/// </summary>
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}
