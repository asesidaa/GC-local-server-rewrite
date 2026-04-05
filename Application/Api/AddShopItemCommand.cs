using Domain.Entities;
using Domain.Enums;
using Shared.Dto.Api;

namespace Application.Api;

public record AddShopItemCommand(UnlockItemType ItemType, int ItemId, int CoinCost) : IRequestWrapper<bool>;

public class AddShopItemCommandHandler : RequestHandlerBase<AddShopItemCommand, bool>
{
    public AddShopItemCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override async Task<ServiceResult<bool>> Handle(AddShopItemCommand request,
        CancellationToken cancellationToken)
    {
        var itemCount = await GetItemCountAsync(request.ItemType, cancellationToken);
        if (request.ItemId < 1 || request.ItemId > itemCount)
            return ServiceResult.Failed<bool>(ServiceError.CustomMessage(
                $"Item ID {request.ItemId} is out of range for {request.ItemType}"));

        if (request.CoinCost < 0)
            return ServiceResult.Failed<bool>(ServiceError.CustomMessage("Coin cost must be non-negative"));

        var typeInt = (int)request.ItemType;
        var exists = await CardDbContext.ShopItems
            .AnyAsync(s => s.ItemType == typeInt && s.ItemId == request.ItemId, cancellationToken);

        if (exists)
            return ServiceResult.Failed<bool>(ServiceError.CustomMessage(
                $"{request.ItemType} #{request.ItemId} is already in the shop"));

        CardDbContext.ShopItems.Add(new ShopItem
        {
            ItemType = typeInt,
            ItemId = request.ItemId,
            CoinCost = request.CoinCost
        });

        await CardDbContext.SaveChangesAsync(cancellationToken);

        return new ServiceResult<bool>(true);
    }
}
