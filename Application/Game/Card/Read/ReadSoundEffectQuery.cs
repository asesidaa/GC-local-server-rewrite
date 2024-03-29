namespace Application.Game.Card.Read;


public record ReadSoundEffectQuery(long CardId) : IRequestWrapper<string>;

public class ReadSoundEffectQueryHandler : RequestHandlerBase<ReadSoundEffectQuery, string>
{
    private const string SOUND_EFFECT_XPATH = "/root/sound_effect/record";
    public ReadSoundEffectQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override Task<ServiceResult<string>> Handle(ReadSoundEffectQuery request, CancellationToken cancellationToken)
    {
        var count = Config.SeCount;
        
        var list = new List<SoundEffectDto>();
        for (int i = 0; i < count; i++)
        {
            var soundEffect = new SoundEffectDto
            {
                Id = i,
                CardId = request.CardId,
                SoundEffectId = i + 1,
                Created = "2013-01-01 08:00:00",
                Modified = "2013-01-01 08:00:00",
                NewFlag = 0,
                UseFlag = 1
            };
            list.Add(soundEffect);
        }

        var result = list.SerializeCardDataList(SOUND_EFFECT_XPATH);

        return Task.FromResult(new ServiceResult<string>(result));  
    }
}
