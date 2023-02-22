namespace Application.Game.Card.Read;


public record ReadNavigatorQuery(long CardId) : IRequestWrapper<string>;

public class ReadNavigatorQueryHandler : RequestHandlerBase<ReadNavigatorQuery, string>
{
    private const string NAVIGATOR_XPATH = "/root/navigator/record";

    public ReadNavigatorQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadNavigatorQuery request, CancellationToken cancellationToken)
    {
        var count = Config.NavigatorCount;
        
        var list = new List<NavigatorDto>();
        for (int i = 0; i < count; i++)
        {
            var navigator = new NavigatorDto
            {
                Id = i,
                CardId = request.CardId,
                NavigatorId = i + 1,
                Created = "2013-01-01 08:00:00",
                Modified = "2013-01-01 08:00:00",
                NewFlag = 0,
                UseFlag = 1
            };
            list.Add(navigator);
        }

        var result = list.SerializeCardDataList(NAVIGATOR_XPATH);

        return Task.FromResult(new ServiceResult<string>(result));
    }
}
