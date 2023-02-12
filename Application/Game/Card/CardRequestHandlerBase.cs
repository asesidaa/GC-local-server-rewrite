using Application.Common.Models;
using Application.Interfaces;
using Domain.Config;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Game.Card;

public abstract class CardRequestHandlerBase<TIn, TOut>: IRequestHandlerWrapper<TIn, TOut> 
    where TIn : IRequestWrapper<TOut>
{
    protected ICardDbContext CardDbContext { get; }
    protected IMusicDbContext MusicDbContext { get; }
    
    protected GameConfig Config { get; }
    
    public CardRequestHandlerBase(ICardDependencyAggregate aggregate)
    {
        CardDbContext = aggregate.CardDbContext;
        MusicDbContext = aggregate.MusicDbContext;
        Config = aggregate.Options.Value;
    }

    public abstract Task<ServiceResult<TOut>> Handle(TIn request, CancellationToken cancellationToken);
}