namespace Application.Api;

public record RemoveShopItemCommand(int ShopItemId) : IRequestWrapper<bool>;

public class RemoveShopItemCommandHandler : RequestHandlerBase<RemoveShopItemCommand, bool>
{
    public RemoveShopItemCommandHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override async Task<ServiceResult<bool>> Handle(RemoveShopItemCommand request,
        CancellationToken cancellationToken)
    {
        var shopItem = await CardDbContext.ShopItems
            .FirstOrDefaultAsync(s => s.Id == request.ShopItemId, cancellationToken);

        if (shopItem is null)
            return ServiceResult.Failed<bool>(ServiceError.CustomMessage("Shop item not found"));

        CardDbContext.ShopItems.Remove(shopItem);
        await CardDbContext.SaveChangesAsync(cancellationToken);

        return new ServiceResult<bool>(true);
    }
}
