using System.Diagnostics.CodeAnalysis;

namespace Application.Game.Card.Read;


public record ReadMusicQuery(long CardId) : IRequestWrapper<string>;

public class ReadMusicQueryHandler : RequestHandlerBase<ReadMusicQuery, string>
{
    private const string MUSIC_XPATH = "/root/music/record";
    public ReadMusicQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    [SuppressMessage("ReSharper.DPA", "DPA0007: Large number of DB records", 
        Justification = "To return all musics, the whole table need to be returned")]
    public override async Task<ServiceResult<string>> Handle(ReadMusicQuery request, CancellationToken cancellationToken)
    {
        var musics = await MusicDbContext.MusicUnlocks.ToListAsync(cancellationToken: cancellationToken);
        var dtoList = musics.Select((unlock, i) =>
        {
            var dto = unlock.MusicToDto();
            dto.Id = i;
            dto.CalcFlag = dto.NewFlag;
            dto.NewFlag = 0;
            return dto;
        });
        
        var result = dtoList.SerializeCardDataList(MUSIC_XPATH);

        return new ServiceResult<string>(result);
    }
}
