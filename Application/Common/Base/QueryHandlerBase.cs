namespace Application.Common.Base;

public abstract class QueryHandlerBase<TQuery, T> : RequestHandlerBase, IQueryHandlerWrapper<TQuery, T>
    where TQuery : IQueryWrapper<T>
{
    protected QueryHandlerBase(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public abstract ValueTask<ServiceResult<T>> Handle(TQuery query, CancellationToken cancellationToken);
}
