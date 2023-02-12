using Application.Interfaces;
using Domain.Config;
using Microsoft.Extensions.Options;

namespace Application.Game.Card;

public class CardDependencyAggregate : ICardDependencyAggregate
{
    public CardDependencyAggregate(ICardDbContext cardDbContext, IMusicDbContext musicDbContext, IOptions<GameConfig> options)
    {
        CardDbContext = cardDbContext;
        MusicDbContext = musicDbContext;
        Options = options;
    }

    public ICardDbContext CardDbContext { get; }
    public IMusicDbContext MusicDbContext { get; }
    
    public IOptions<GameConfig> Options { get; }
}