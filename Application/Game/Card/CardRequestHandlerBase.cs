using Application.Common.Models;
using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Game.Card;

public abstract class CardRequestHandlerBase<TIn, TOut>: IRequestHandlerWrapper<TIn, TOut> 
    where TIn : IRequestWrapper<TOut>
{
    public ICardDbContext CardDbContext { get; }
    public IMusicDbContext MusicDbContext { get; }
    
    public CardRequestHandlerBase(ICardDependencyAggregate aggregate)
    {
        CardDbContext = aggregate.CardDbContext;
        MusicDbContext = aggregate.MusicDbContext;
    }

    public abstract Task<ServiceResult<TOut>> Handle(TIn request, CancellationToken cancellationToken);
}