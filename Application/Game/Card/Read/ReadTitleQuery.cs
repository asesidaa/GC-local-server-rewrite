using Application.Common.Helpers;
using Domain.Enums;

namespace Application.Game.Card.Read;


public record ReadTitleQuery(long CardId) : IRequestWrapper<string>;

public class ReadTitleQueryHandler : RequestHandlerBase<ReadTitleQuery, string>
{
    private const string TITLE_XPATH = "/root/title/record";

    public ReadTitleQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override async Task<ServiceResult<string>> Handle(ReadTitleQuery request, CancellationToken cancellationToken)
    {
        var count = Config.TitleCount;
        var bitset = await LoadBitset(request.CardId, UnlockItemType.Title, count, cancellationToken);

        var list = new List<TitleDto>();
        for (int i = 0; i < count; i++)
        {
            var title = new TitleDto
            {
                Id = i,
                CardId = request.CardId,
                TitleId = i + 1,
                Created = "2013-01-01 08:00:00",
                Modified = "2013-01-01 08:00:00",
                NewFlag = 0,
                UseFlag = bitset is null ? 1 : BitsetHelper.IsUnlocked(bitset, i + 1) ? 1 : 0
            };
            list.Add(title);
        }

        var result = list.SerializeCardDataList(TITLE_XPATH);

        return new ServiceResult<string>(result);
    }
}
