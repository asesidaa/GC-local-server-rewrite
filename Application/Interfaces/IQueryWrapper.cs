using Mediator;
using Shared.Models;

namespace Application.Interfaces;

public interface IQueryWrapper<T> : IQuery<ServiceResult<T>>;

public interface IQueryHandlerWrapper<TQuery, T> : IQueryHandler<TQuery, ServiceResult<T>>
    where TQuery : IQueryWrapper<T>;
