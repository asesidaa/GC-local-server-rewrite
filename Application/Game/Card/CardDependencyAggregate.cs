using Application.Interfaces;

namespace Application.Game.Card;

public class CardDependencyAggregate : ICardDependencyAggregate
{
    public CardDependencyAggregate(ICardDbContext cardDbContext, IMusicDbContext musicDbContext)
    {
        CardDbContext = cardDbContext;
        MusicDbContext = musicDbContext;
    }

    public ICardDbContext CardDbContext { get; }
    public IMusicDbContext MusicDbContext { get; }
}