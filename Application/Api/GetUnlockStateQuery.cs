using Application.Common.Helpers;
using Domain.Config;
using Domain.Enums;
using Shared.Dto.Api;

namespace Application.Api;

public record GetUnlockStateQuery(long CardId) : IRequestWrapper<List<UnlockStateDto>>;

public class GetUnlockStateQueryHandler : RequestHandlerBase<GetUnlockStateQuery, List<UnlockStateDto>>
{
    public GetUnlockStateQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override async Task<ServiceResult<List<UnlockStateDto>>> Handle(GetUnlockStateQuery request,
        CancellationToken cancellationToken)
    {
        var playerStates = await CardDbContext.PlayerUnlockStates
            .AsNoTracking()
            .Where(s => s.CardId == request.CardId)
            .ToDictionaryAsync(s => s.ItemType, s => s.UnlockedBitset, cancellationToken);

        var itemTypes = new (UnlockItemType Type, string Name, int Count)[]
        {
            (UnlockItemType.Avatar, "Avatar", Config.AvatarCount),
            (UnlockItemType.Navigator, "Navigator", Config.NavigatorCount),
            (UnlockItemType.Item, "Item", Config.ItemCount),
            (UnlockItemType.Skin, "Skin", Config.SkinCount),
            (UnlockItemType.SoundEffect, "SoundEffect", Config.SeCount),
            (UnlockItemType.Title, "Title", Config.TitleCount),
        };

        var result = new List<UnlockStateDto>();

        foreach (var (type, name, count) in itemTypes)
        {
            var typeInt = (int)type;
            var bitset = playerStates.TryGetValue(typeInt, out var json)
                ? BitsetHelper.EnsureLength(BitsetHelper.Deserialize(json), count)
                : BitsetHelper.CreateAllZeroes(count);

            result.Add(new UnlockStateDto
            {
                ItemType = name,
                TotalCount = count,
                UnlockedCount = BitsetHelper.CountUnlocked(bitset, count)
            });
        }

        // Music
        var maxMusicId = await MusicDbContext.MusicUnlocks.AnyAsync(cancellationToken)
            ? await MusicDbContext.MusicUnlocks.MaxAsync(m => (int)m.MusicId, cancellationToken)
            : 0;

        if (maxMusicId > 0)
        {
            var musicCount = await MusicDbContext.MusicUnlocks.CountAsync(cancellationToken);
            var musicTypeInt = (int)UnlockItemType.Music;
            var musicBitset = playerStates.TryGetValue(musicTypeInt, out var musicJson)
                ? BitsetHelper.EnsureLength(BitsetHelper.Deserialize(musicJson), maxMusicId)
                : BitsetHelper.CreateAllZeroes(maxMusicId);

            var musicIds = await MusicDbContext.MusicUnlocks
                .Where(m => m.UseFlag)
                .Select(m => (int)m.MusicId)
                .ToListAsync(cancellationToken);

            var unlockedMusicCount = musicIds.Count(id => BitsetHelper.IsUnlocked(musicBitset, id));

            result.Add(new UnlockStateDto
            {
                ItemType = "Music",
                TotalCount = musicCount,
                UnlockedCount = unlockedMusicCount
            });
        }

        return new ServiceResult<List<UnlockStateDto>>(result);
    }
}
