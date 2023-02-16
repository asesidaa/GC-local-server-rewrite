namespace Application.Game.Card.Read;


public record ReadSkinQuery(long CardId) : IRequestWrapper<string>;

public class ReadSkinQueryHandler : CardRequestHandlerBase<ReadSkinQuery, string>
{
    private const string SKIN_XPATH = "/root/skin/record";
    
    public ReadSkinQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadSkinQuery request, CancellationToken cancellationToken)
    {
        var count = Config.SkinCount;
        
        var list = new List<SkinDto>();
        for (int i = 0; i < count; i++)
        {
            var skin = new SkinDto
            {
                Id = i,
                CardId = request.CardId,
                SkinId = i + 1,
                Created = "2013-01-01 08:00:00",
                Modified = "2013-01-01 08:00:00",
                NewFlag = 0,
                UseFlag = 1
            };
            list.Add(skin);
        }

        var result = list.SerializeCardDataList(SKIN_XPATH);

        return Task.FromResult(new ServiceResult<string>(result));  
    }
}
