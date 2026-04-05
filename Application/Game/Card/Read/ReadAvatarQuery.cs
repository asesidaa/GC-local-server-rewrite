using Application.Common.Helpers;
using Domain.Enums;

namespace Application.Game.Card.Read;


public record ReadAvatarQuery(long CardId) : IRequestWrapper<string>;

public class ReadAvatarQueryHandler : RequestHandlerBase<ReadAvatarQuery, string>
{
    private const string AVATAR_XPATH = "/root/avatar/record";

    public ReadAvatarQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override async Task<ServiceResult<string>> Handle(ReadAvatarQuery request, CancellationToken cancellationToken)
    {
        var count = Config.AvatarCount;
        var bitset = await LoadBitset(request.CardId, UnlockItemType.Avatar, count, cancellationToken);

        var list = new List<AvatarDto>();
        for (int i = 0; i < count; i++)
        {
            var avatar = new AvatarDto
            {
                Id = i,
                CardId = request.CardId,
                AvatarId = i + 1,
                Created = "2013-01-01 08:00:00",
                Modified = "2013-01-01 08:00:00",
                NewFlag = 0,
                UseFlag = bitset is null ? 1 : BitsetHelper.IsUnlocked(bitset, i + 1) ? 1 : 0
            };
            list.Add(avatar);
        }

        var result = list.SerializeCardDataList(AVATAR_XPATH);

        return new ServiceResult<string>(result);
    }
}
