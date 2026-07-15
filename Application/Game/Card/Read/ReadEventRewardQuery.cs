namespace Application.Game.Card.Read;


public record ReadEventRewardQuery(long CardId) : IQueryWrapper<string>;

public class ReadEventRewardQueryHandler : QueryHandlerBase<ReadEventRewardQuery, string>
{
    private const string EVENT_REWARD_XPATH = "/root/event_reward";
    
    public ReadEventRewardQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override ValueTask<ServiceResult<string>> Handle(ReadEventRewardQuery request, CancellationToken cancellationToken)
    {
        var result = new object().SerializeCardData(EVENT_REWARD_XPATH);

        return ValueTask.FromResult(new ServiceResult<string>(result));
    }
}
