namespace Application.Game.Card.Read;


public record ReadEventRewardQuery(long CardId) : IRequestWrapper<string>;

public class ReadEventRewardQueryHandler : CardRequestHandlerBase<ReadEventRewardQuery, string>
{
    private const string EVENT_REWARD_XPATH = "/root/event_reward";
    
    public ReadEventRewardQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadEventRewardQuery request, CancellationToken cancellationToken)
    {
        var result = new object().SerializeCardData(EVENT_REWARD_XPATH);

        return Task.FromResult(new ServiceResult<string>(result));
    }
}
