using Application.Common.Helpers;
using Domain.Entities;
using Domain.Enums;
using Shared.Dto.Api;

namespace Application.Api;

public record PurchaseItemCommand(long CardId, int ShopItemId) : IRequestWrapper<PlayerCoinDto>;

public class PurchaseItemCommandHandler : RequestHandlerBase<PurchaseItemCommand, PlayerCoinDto>
{
    public PurchaseItemCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override async Task<ServiceResult<PlayerCoinDto>> Handle(PurchaseItemCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load shop item
        var shopItem = await CardDbContext.ShopItems
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.ShopItemId, cancellationToken);

        if (shopItem is null)
            return ServiceResult.Failed<PlayerCoinDto>(ServiceError.CustomMessage("Shop item not found"));

        // 2. Load or create player coin balance
        var coin = await CardDbContext.PlayerCoins
            .FirstOrDefaultAsync(c => c.CardId == request.CardId, cancellationToken);

        if (coin is null)
        {
            coin = new PlayerCoin
            {
                CardId = request.CardId,
                CurrentCoins = UnlockConfig.DefaultCoins,
                TotalCoins = UnlockConfig.DefaultCoins,
                MonthlyCoins = UnlockConfig.DefaultCoins
            };
            CardDbContext.PlayerCoins.Add(coin);
        }

        // 3. Validate sufficient coins
        if (coin.CurrentCoins < shopItem.CoinCost)
            return ServiceResult.Failed<PlayerCoinDto>(ServiceError.CustomMessage("Insufficient coins"));

        // 4. Deduct coins
        coin.CurrentCoins -= shopItem.CoinCost;

        // 5. Unlock the item (load/create bitset, set bit)
        var itemType = (UnlockItemType)shopItem.ItemType;
        var typeInt = shopItem.ItemType;
        var itemCount = await GetItemCountAsync(itemType, cancellationToken);

        var state = await CardDbContext.PlayerUnlockStates
            .FirstOrDefaultAsync(s => s.CardId == request.CardId && s.ItemType == typeInt, cancellationToken);

        if (state is null)
        {
            // Fall back to default unlock state as starting point
            var defaultState = await CardDbContext.DefaultUnlockStates
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ItemType == typeInt, cancellationToken);

            state = new PlayerUnlockState
            {
                CardId = request.CardId,
                ItemType = typeInt,
                UnlockedBitset = defaultState?.UnlockedBitset ?? BitsetHelper.Serialize(BitsetHelper.CreateAllZeroes(itemCount))
            };
            CardDbContext.PlayerUnlockStates.Add(state);
        }

        var bitset = BitsetHelper.EnsureLength(BitsetHelper.Deserialize(state.UnlockedBitset), itemCount);

        // Prevent double-purchase: if item is already unlocked, don't charge again
        if (BitsetHelper.IsUnlocked(bitset, shopItem.ItemId))
            return ServiceResult.Failed<PlayerCoinDto>(ServiceError.CustomMessage("Item is already unlocked"));

        BitsetHelper.SetBit(bitset, shopItem.ItemId, true);
        state.UnlockedBitset = BitsetHelper.Serialize(bitset);

        // 6. Single SaveChanges (atomic within same DbContext)
        await CardDbContext.SaveChangesAsync(cancellationToken);

        return new ServiceResult<PlayerCoinDto>(new PlayerCoinDto
        {
            CardId = request.CardId,
            CurrentCoins = coin.CurrentCoins,
            TotalCoins = coin.TotalCoins,
            MonthlyCoins = coin.MonthlyCoins
        });
    }
}
