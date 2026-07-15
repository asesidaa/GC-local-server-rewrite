using Shared.Dto.Api;

namespace Application.Api;

public record GetMusicListQuery : IQueryWrapper<List<MusicItemDto>>;

public class GetMusicListQueryHandler : QueryHandlerBase<GetMusicListQuery, List<MusicItemDto>>
{
    public GetMusicListQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override async ValueTask<ServiceResult<List<MusicItemDto>>> Handle(GetMusicListQuery request,
        CancellationToken cancellationToken)
    {
        var musicList = await MusicDbContext.MusicUnlocks
            .AsNoTracking()
            .OrderBy(m => m.MusicId)
            .Select(m => new MusicItemDto
            {
                MusicId = m.MusicId,
                Title = m.Title,
                Artist = m.Artist,
                UseFlag = m.UseFlag
            })
            .ToListAsync(cancellationToken);

        return new ServiceResult<List<MusicItemDto>>(musicList);
    }
}
