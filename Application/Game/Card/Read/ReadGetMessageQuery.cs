namespace Application.Game.Card.Read;


public record ReadGetMessageQuery(long CardId) : IQueryWrapper<string>;

public class ReadGetMessageQueryHandler : QueryHandlerBase<ReadGetMessageQuery, string>
{
    private const string GET_MESSAGE_XPATH = "/root/get_message";
    
    public ReadGetMessageQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override ValueTask<ServiceResult<string>> Handle(ReadGetMessageQuery request, CancellationToken cancellationToken)
    {
        var result = new object().SerializeCardData(GET_MESSAGE_XPATH);

        return ValueTask.FromResult(new ServiceResult<string>(result));
    }
}
