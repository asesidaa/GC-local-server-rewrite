using Domain.Enums;
using Shared.Dto.Api;

namespace Application.Api;

public record GetShopItemsQuery : IRequestWrapper<List<ShopItemDto>>;

public class GetShopItemsQueryHandler : RequestHandlerBase<GetShopItemsQuery, List<ShopItemDto>>
{
    public GetShopItemsQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override async Task<ServiceResult<List<ShopItemDto>>> Handle(GetShopItemsQuery request,
        CancellationToken cancellationToken)
    {
        var shopItems = await CardDbContext.ShopItems
            .AsNoTracking()
            .OrderBy(s => s.ItemType)
            .ThenBy(s => s.ItemId)
            .ToListAsync(cancellationToken);

        // Build music name lookup for Music-type shop items
        var musicNames = new Dictionary<int, string>();
        if (shopItems.Any(s => s.ItemType == (int)UnlockItemType.Music))
        {
            musicNames = await MusicDbContext.MusicUnlocks
                .AsNoTracking()
                .ToDictionaryAsync(m => (int)m.MusicId, m => m.Title, cancellationToken);
        }

        var result = shopItems.Select(s =>
        {
            var itemType = (UnlockItemType)s.ItemType;
            var itemName = itemType == UnlockItemType.Music && musicNames.TryGetValue(s.ItemId, out var musicTitle)
                ? musicTitle
                : string.Empty; // Non-music names are resolved client-side via DataService

            return new ShopItemDto
            {
                Id = s.Id,
                ItemType = itemType.ToString(),
                ItemId = s.ItemId,
                ItemName = itemName,
                CoinCost = s.CoinCost
            };
        }).ToList();

        return new ServiceResult<List<ShopItemDto>>(result);
    }
}
