namespace Application.Game.Card.Read;


public record ReadCondQuery(long CardId) : IRequestWrapper<string>;

public class ReadCondQueryHandler : RequestHandlerBase<ReadCondQuery, string>
{
    private const string COND_XPATH = "/root/cond";
    
    public ReadCondQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadCondQuery request, CancellationToken cancellationToken)
    {
        var result = new object().SerializeCardData(COND_XPATH);

        return Task.FromResult(new ServiceResult<string>(result));
    }
}
