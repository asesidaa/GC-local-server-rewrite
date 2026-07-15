namespace Application.Game.Card.Read;


public record ReadTotalTrophyQuery(long CardId) : IQueryWrapper<string>;

public class ReadTotalTrophyQueryHandler : QueryHandlerBase<ReadTotalTrophyQuery, string>
{
    private const string TOTAL_TROPHY_XPATH = "/root/total_trophy";
    
    public ReadTotalTrophyQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override ValueTask<ServiceResult<string>> Handle(ReadTotalTrophyQuery request, CancellationToken cancellationToken)
    {
        var trophy = new TotalTrophyDto
        {
            CardId = request.CardId,
            TrophyNum = 8
        };

        var result = trophy.SerializeCardData(TOTAL_TROPHY_XPATH);

        return ValueTask.FromResult(new ServiceResult<string>(result));
    }
}
