namespace Application.Game.Card.Read;


public record ReadAvatarQuery(long CardId) : IRequestWrapper<string>;

public class ReadAvatarQueryHandler : RequestHandlerBase<ReadAvatarQuery, string>
{
    private const string AVATAR_XPATH = "/root/avatar/record";

    public ReadAvatarQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadAvatarQuery request, CancellationToken cancellationToken)
    {
        var count = Config.AvatarCount;
        var list = new List<AvatarDto>();
        for (int i = 0; i < count; i++)
        {
            var avatar = new AvatarDto
            {
                Id = i,
                CardId = request.CardId,
                AvatarId = i + 1,
                Created = "2013-01-01 08:00:00",
                Modified = "2013-01-01 08:00:00",
                NewFlag = 0,
                UseFlag = 1
            };
            list.Add(avatar);
        }

        var result = list.SerializeCardDataList(AVATAR_XPATH);

        return Task.FromResult(new ServiceResult<string>(result));
    }
}
