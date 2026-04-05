using Application.Common.Helpers;
using Domain.Enums;

namespace Application.Game.Card.Read;

public record ReadItemQuery(long CardId) : IRequestWrapper<string>;

public class ReadItemQueryHandler : RequestHandlerBase<ReadItemQuery, string>
{
    private const string ITEM_XPATH = "/root/item/record";

    public ReadItemQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override async Task<ServiceResult<string>> Handle(ReadItemQuery request, CancellationToken cancellationToken)
    {
        var count = Config.ItemCount;
        var bitset = await LoadBitset(request.CardId, UnlockItemType.Item, count, cancellationToken);

        var list = new List<ItemDto>();
        for (int i = 0; i < count; i++)
        {
            var item = new ItemDto
            {
                Id = i,
                CardId = request.CardId,
                ItemId = i + 1,
                ItemNum = 90,
                Created = "2013-01-01",
                Modified = "2013-01-01",
                NewFlag = 0,
                UseFlag = bitset is null ? 1 : BitsetHelper.IsUnlocked(bitset, i + 1) ? 1 : 0
            };
            list.Add(item);
        }

        var result = list.SerializeCardDataList(ITEM_XPATH);

        return new ServiceResult<string>(result);
    }
}
