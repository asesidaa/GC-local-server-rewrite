namespace Application.Game.Card.Read;


public record ReadMusicExtraQuery(long CardId) : IRequestWrapper<string>;

public class ReadMusicExtraQueryHandler : RequestHandlerBase<ReadMusicExtraQuery, string>
{
    private const string MUSIC_EXTRA_XPATH = "/root/music_extra";

    private const string RECORD_XPATH = $"{MUSIC_EXTRA_XPATH}/record";
    
    public ReadMusicExtraQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override async Task<ServiceResult<string>> Handle(ReadMusicExtraQuery request, CancellationToken cancellationToken)
    {
        var musics = await MusicDbContext.MusicExtras.ToListAsync(cancellationToken: cancellationToken);
        var dtoList = musics.Select((aou, i) =>
        {
            var dto = aou.MusicExtraToDto();
            dto.Id = i;
            return dto;
        }).ToList();

        var result = dtoList.Count == 0 ? new object().SerializeCardData(MUSIC_EXTRA_XPATH) 
            : dtoList.SerializeCardDataList(RECORD_XPATH);

        return new ServiceResult<string>(result);
    }
}
