namespace Application.Game.Card.Read;


public record ReadTotalTrophyQuery(long CardId) : IRequestWrapper<string>;

public class ReadTotalTrophyQueryHandler : RequestHandlerBase<ReadTotalTrophyQuery, string>
{
    private const string TOTAL_TROPHY_XPATH = "/root/total_trophy";
    
    public ReadTotalTrophyQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadTotalTrophyQuery request, CancellationToken cancellationToken)
    {
        var trophy = new TotalTrophyDto
        {
            CardId = request.CardId,
            TrophyNum = 8
        };

        var result = trophy.SerializeCardData(TOTAL_TROPHY_XPATH);

        return Task.FromResult(new ServiceResult<string>(result));
    }
}
