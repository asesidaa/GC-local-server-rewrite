using Application.Common.Helpers;
using Domain.Config;
using Domain.Enums;

namespace Application.Common.Base;

public abstract class RequestHandlerBase
{
    protected ICardDbContext CardDbContext { get; }
    protected IMusicDbContext MusicDbContext { get; }
    protected GameConfig Config { get; }
    protected UnlockConfig UnlockConfig { get; }

    protected RequestHandlerBase(ICardDependencyAggregate aggregate)
    {
        CardDbContext = aggregate.CardDbContext;
        MusicDbContext = aggregate.MusicDbContext;
        Config = aggregate.Options.Value;
        UnlockConfig = aggregate.UnlockOptions.Value;
    }

    protected async Task<long[]?> LoadBitset(long cardId, UnlockItemType itemType, int itemCount,
        CancellationToken cancellationToken)
    {
        if (!UnlockConfig.EnableLockSystem)
            return null;

        var typeInt = (int)itemType;
        var playerState = await CardDbContext.PlayerUnlockStates
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CardId == cardId && s.ItemType == typeInt, cancellationToken);

        long[] bitset;
        if (playerState is not null)
        {
            bitset = BitsetHelper.Deserialize(playerState.UnlockedBitset);
        }
        else
        {
            var defaultState = await CardDbContext.DefaultUnlockStates
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ItemType == typeInt, cancellationToken);

            bitset = defaultState is not null
                ? BitsetHelper.Deserialize(defaultState.UnlockedBitset)
                : BitsetHelper.CreateAllZeroes(itemCount);
        }

        return BitsetHelper.EnsureLength(bitset, itemCount);
    }

    protected async Task<int> GetItemCountAsync(UnlockItemType itemType, CancellationToken cancellationToken) => itemType switch
    {
        UnlockItemType.Avatar => Config.AvatarCount,
        UnlockItemType.Navigator => Config.NavigatorCount,
        UnlockItemType.Item => Config.ItemCount,
        UnlockItemType.Skin => Config.SkinCount,
        UnlockItemType.SoundEffect => Config.SeCount,
        UnlockItemType.Title => Config.TitleCount,
        UnlockItemType.Music => await MusicDbContext.MusicUnlocks.AnyAsync(cancellationToken)
            ? await MusicDbContext.MusicUnlocks.MaxAsync(m => (int)m.MusicId, cancellationToken)
            : 0,
        _ => throw new ArgumentOutOfRangeException(nameof(itemType))
    };
}
