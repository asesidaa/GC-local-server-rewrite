namespace Application.Common.Base;

public abstract class CommandHandlerBase<TCommand, T> : RequestHandlerBase, ICommandHandlerWrapper<TCommand, T>
    where TCommand : ICommandWrapper<T>
{
    protected CommandHandlerBase(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public abstract ValueTask<ServiceResult<T>> Handle(TCommand command, CancellationToken cancellationToken);
}
