using Domain.Config;
using Microsoft.Extensions.Options;

namespace Application.Game.Card;

public class CardDependencyAggregate : ICardDependencyAggregate
{
    public CardDependencyAggregate(ICardDbContext cardDbContext, IMusicDbContext musicDbContext,
        IOptions<GameConfig> options, IOptions<UnlockConfig> unlockOptions)
    {
        CardDbContext = cardDbContext;
        MusicDbContext = musicDbContext;
        Options = options;
        UnlockOptions = unlockOptions;
    }

    public ICardDbContext CardDbContext { get; }
    public IMusicDbContext MusicDbContext { get; }

    public IOptions<GameConfig> Options { get; }
    public IOptions<UnlockConfig> UnlockOptions { get; }
}