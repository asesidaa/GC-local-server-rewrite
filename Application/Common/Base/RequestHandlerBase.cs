using Domain.Config;

namespace Application.Common.Base;

public abstract class RequestHandlerBase<TIn, TOut>: IRequestHandlerWrapper<TIn, TOut> 
    where TIn : IRequestWrapper<TOut>
{
    protected ICardDbContext CardDbContext { get; }
    protected IMusicDbContext MusicDbContext { get; }
    
    protected GameConfig Config { get; }
    
    public RequestHandlerBase(ICardDependencyAggregate aggregate)
    {
        CardDbContext = aggregate.CardDbContext;
        MusicDbContext = aggregate.MusicDbContext;
        Config = aggregate.Options.Value;
    }

    public abstract Task<ServiceResult<TOut>> Handle(TIn request, CancellationToken cancellationToken);
}