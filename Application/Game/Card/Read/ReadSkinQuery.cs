using Application.Common.Helpers;
using Domain.Enums;

namespace Application.Game.Card.Read;


public record ReadSkinQuery(long CardId) : IRequestWrapper<string>;

public class ReadSkinQueryHandler : RequestHandlerBase<ReadSkinQuery, string>
{
    private const string SKIN_XPATH = "/root/skin/record";

    public ReadSkinQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override async Task<ServiceResult<string>> Handle(ReadSkinQuery request, CancellationToken cancellationToken)
    {
        var count = Config.SkinCount;
        var bitset = await LoadBitset(request.CardId, UnlockItemType.Skin, count, cancellationToken);

        var list = new List<SkinDto>();
        for (int i = 0; i < count; i++)
        {
            var skin = new SkinDto
            {
                Id = i,
                CardId = request.CardId,
                SkinId = i + 1,
                Created = "2013-01-01 08:00:00",
                Modified = "2013-01-01 08:00:00",
                NewFlag = 0,
                UseFlag = bitset is null ? 1 : BitsetHelper.IsUnlocked(bitset, i + 1) ? 1 : 0
            };
            list.Add(skin);
        }

        var result = list.SerializeCardDataList(SKIN_XPATH);

        return new ServiceResult<string>(result);
    }
}
