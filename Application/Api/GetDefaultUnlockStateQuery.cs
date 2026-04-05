using Application.Common.Helpers;
using Domain.Enums;
using Shared.Dto.Api;

namespace Application.Api;

public record GetDefaultUnlockStateQuery : IRequestWrapper<List<DefaultUnlockStateDto>>;

public class GetDefaultUnlockStateQueryHandler : RequestHandlerBase<GetDefaultUnlockStateQuery, List<DefaultUnlockStateDto>>
{
    public GetDefaultUnlockStateQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public override async Task<ServiceResult<List<DefaultUnlockStateDto>>> Handle(GetDefaultUnlockStateQuery request,
        CancellationToken cancellationToken)
    {
        var defaults = await CardDbContext.DefaultUnlockStates
            .AsNoTracking()
            .ToDictionaryAsync(d => d.ItemType, d => d.UnlockedBitset, cancellationToken);

        var itemTypes = new (UnlockItemType Type, string Name, int Count)[]
        {
            (UnlockItemType.Avatar, "Avatar", Config.AvatarCount),
            (UnlockItemType.Navigator, "Navigator", Config.NavigatorCount),
            (UnlockItemType.Item, "Item", Config.ItemCount),
            (UnlockItemType.Skin, "Skin", Config.SkinCount),
            (UnlockItemType.SoundEffect, "SoundEffect", Config.SeCount),
            (UnlockItemType.Title, "Title", Config.TitleCount),
        };

        var result = new List<DefaultUnlockStateDto>();

        foreach (var (type, name, count) in itemTypes)
        {
            var typeInt = (int)type;
            var bitset = defaults.TryGetValue(typeInt, out var json)
                ? BitsetHelper.EnsureLength(BitsetHelper.Deserialize(json), count)
                : BitsetHelper.CreateAllZeroes(count);

            result.Add(new DefaultUnlockStateDto
            {
                ItemType = name,
                TotalCount = count,
                UnlockedCount = BitsetHelper.CountUnlocked(bitset, count)
            });
        }

        // Music
        if (await MusicDbContext.MusicUnlocks.AnyAsync(cancellationToken))
        {
            var musicCount = await MusicDbContext.MusicUnlocks.CountAsync(cancellationToken);
            var maxMusicId = await MusicDbContext.MusicUnlocks.MaxAsync(m => (int)m.MusicId, cancellationToken);
            var musicTypeInt = (int)UnlockItemType.Music;
            var musicBitset = defaults.TryGetValue(musicTypeInt, out var musicJson)
                ? BitsetHelper.EnsureLength(BitsetHelper.Deserialize(musicJson), maxMusicId)
                : BitsetHelper.CreateAllZeroes(maxMusicId);

            var musicIds = await MusicDbContext.MusicUnlocks
                .Where(m => m.UseFlag)
                .Select(m => (int)m.MusicId)
                .ToListAsync(cancellationToken);

            result.Add(new DefaultUnlockStateDto
            {
                ItemType = "Music",
                TotalCount = musicCount,
                UnlockedCount = musicIds.Count(id => BitsetHelper.IsUnlocked(musicBitset, id))
            });
        }

        return new ServiceResult<List<DefaultUnlockStateDto>>(result);
    }
}
