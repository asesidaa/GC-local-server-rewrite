using System.Diagnostics.CodeAnalysis;

namespace Application.Game.Card.Read;


public record ReadMusicAouQuery(long CardId) : IRequestWrapper<string>;

public class ReadMusicAouQueryHandler : RequestHandlerBase<ReadMusicAouQuery, string>
{
    private const string MUSIC_AOU_XPATH = "/root/music_aou";

    private const string RECORD_XPATH = $"{MUSIC_AOU_XPATH}/record";
    
    public ReadMusicAouQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    [SuppressMessage("ReSharper.DPA", "DPA0007: Large number of DB records", 
        Justification = "To return all musics, the whole table need to be returned")]
    public override async Task<ServiceResult<string>> Handle(ReadMusicAouQuery request, CancellationToken cancellationToken)
    {
        var musics = await MusicDbContext.MusicAous.ToListAsync(cancellationToken: cancellationToken);
        var dtoList = musics.Select((aou, i) =>
        {
            var dto = aou.MusicAouToDto();
            dto.Id = i;
            return dto;
        }).ToList();

        var result = dtoList.Count == 0 ? new object().SerializeCardData(MUSIC_AOU_XPATH) 
            : dtoList.SerializeCardDataList(RECORD_XPATH);

        return new ServiceResult<string>(result);
    }
}
