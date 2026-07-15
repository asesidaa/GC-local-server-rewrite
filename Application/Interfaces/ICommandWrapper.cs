using Mediator;
using Shared.Models;

namespace Application.Interfaces;

public interface ICommandWrapper<T> : ICommand<ServiceResult<T>>;

public interface ICommandHandlerWrapper<TCommand, T> : ICommandHandler<TCommand, ServiceResult<T>>
    where TCommand : ICommandWrapper<T>;
