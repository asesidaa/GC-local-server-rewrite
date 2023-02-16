namespace Application.Game.Card.Read;


public record ReadUnlockKeynumQuery(long CardId) : IRequestWrapper<string>;

public class ReadUnlockKeynumQueryHandler : RequestHandlerBase<ReadUnlockKeynumQuery, string>
{
    private const string UNLOCK_KEYNUM_XPATH = "/root/unlock_keynum/record";
    public ReadUnlockKeynumQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadUnlockKeynumQuery request, CancellationToken cancellationToken)
    {
        var unlockables = Config.UnlockRewards;
        var list = unlockables.Select((unlockable, index) => new UnlockKeyNumDto
            {
                Id = index,
                CardId = request.CardId,
                RewardId = unlockable.RewardId,
                KeyNum = 0,
                RewardCount = 0,
                CashFlag = 0,
                ExpiredFlag = 0,
                UseFlag = 1,
                Created = "2013-01-01 08:00:00",
                Modified = "2020-01-01 08:00:00"
            })
            .ToList();
        var result = list.SerializeCardDataList(UNLOCK_KEYNUM_XPATH);

        return Task.FromResult(new ServiceResult<string>(result));

    }
}
