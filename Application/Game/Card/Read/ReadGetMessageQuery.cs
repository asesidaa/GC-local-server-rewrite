namespace Application.Game.Card.Read;


public record ReadGetMessageQuery(long CardId) : IRequestWrapper<string>;

public class ReadGetMessageQueryHandler : RequestHandlerBase<ReadGetMessageQuery, string>
{
    private const string GET_MESSAGE_XPATH = "/root/get_message";
    
    public ReadGetMessageQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadGetMessageQuery request, CancellationToken cancellationToken)
    {
        var result = new object().SerializeCardData(GET_MESSAGE_XPATH);

        return Task.FromResult(new ServiceResult<string>(result)); 
    }
}
