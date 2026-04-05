using System.Diagnostics.CodeAnalysis;
using Application.Common.Helpers;
using Domain.Enums;

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

        // For music, bitset is indexed by MusicId directly (sparse IDs)
        long[]? bitset = null;
        if (UnlockConfig.EnableLockSystem && musics.Count > 0)
        {
            var maxMusicId = musics.Max(m => (int)m.MusicId);
            bitset = await LoadBitset(request.CardId, UnlockItemType.Music, maxMusicId, cancellationToken);
        }

        var dtoList = musics.Select((unlock, i) =>
        {
            var dto = unlock.MusicToDto();
            dto.Id = i;

            // Per-player unlock ANDed with global UseFlag
            if (bitset is not null)
            {
                dto.UseFlag = dto.UseFlag == 1 && BitsetHelper.IsUnlocked(bitset, (int)unlock.MusicId) ? 1 : 0;
            }

            return dto;
        });

        var result = dtoList.SerializeCardDataList(MUSIC_XPATH);

        return new ServiceResult<string>(result);
    }
}
