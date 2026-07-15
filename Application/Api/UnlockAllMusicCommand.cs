using System.Diagnostics.CodeAnalysis;
using Application.Common.Helpers;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Api;

public record UnlockAllMusicCommand(long CardId) : ICommandWrapper<bool>;

public class UnlockAllMusicCommandHandler : CommandHandlerBase<UnlockAllMusicCommand, bool>
{
    private readonly ILogger<UnlockAllMusicCommandHandler> logger;

    public UnlockAllMusicCommandHandler(ICardDependencyAggregate aggregate,
        ILogger<UnlockAllMusicCommandHandler> logger) : base(aggregate)
    {
        this.logger = logger;
    }

    [SuppressMessage("ReSharper.DPA", "DPA0007: Large number of DB records")]
    [SuppressMessage("ReSharper.DPA", "DPA0006: Large number of DB commands")]
    public override async ValueTask<ServiceResult<bool>> Handle(UnlockAllMusicCommand request, CancellationToken cancellationToken)
    {
        var exists = await CardDbContext.CardDetails.AnyAsync(
            detail => detail.CardId == request.CardId, cancellationToken);

        if (!exists)
        {
            logger.LogWarning("Attempt to unlock for card {Card} that does not exist or is empty!", request.CardId);
            return ServiceResult.Failed<bool>(ServiceError.CustomMessage("Unlock failed"));
        }

        // When lock system is enabled, set all bits in the Music bitmap
        if (UnlockConfig.EnableLockSystem)
        {
            var maxMusicId = await MusicDbContext.MusicUnlocks.AnyAsync(cancellationToken)
                ? await MusicDbContext.MusicUnlocks.MaxAsync(m => (int)m.MusicId, cancellationToken)
                : 0;

            if (maxMusicId > 0)
            {
                var typeInt = (int)UnlockItemType.Music;
                var state = await CardDbContext.PlayerUnlockStates
                    .FirstOrDefaultAsync(s => s.CardId == request.CardId && s.ItemType == typeInt, cancellationToken);

                if (state is null)
                {
                    state = new PlayerUnlockState
                    {
                        CardId = request.CardId,
                        ItemType = typeInt
                    };
                    CardDbContext.PlayerUnlockStates.Add(state);
                }

                state.UnlockedBitset = BitsetHelper.Serialize(BitsetHelper.CreateAllOnes(maxMusicId));
            }
        }

        // Also maintain CardDetail rows for compatibility
        var musicIds = Config.UnlockRewards
            .Where(config => config.RewardType == RewardType.Music)
            .Select(config => (long)config.TargetId)
            .Distinct()
            .ToArray();

        if (musicIds.Length > 0)
        {
            var existingDetails = await CardDbContext.CardDetails
                .Where(detail => detail.CardId == request.CardId &&
                                 detail.Pcol1 == 10 &&
                                 detail.Pcol3 == 0 &&
                                 musicIds.Contains(detail.Pcol2))
                .ToDictionaryAsync(detail => detail.Pcol2, cancellationToken);
            var playedMusicIds = await CardDbContext.CardDetails
                .Where(detail => detail.CardId == request.CardId &&
                                 detail.Pcol1 == 20 &&
                                 musicIds.Contains(detail.Pcol2))
                .Select(detail => detail.Pcol2)
                .Distinct()
                .ToHashSetAsync(cancellationToken);

            foreach (var musicId in musicIds)
            {
                var unplayedFlag = playedMusicIds.Contains(musicId) ? 0 : 1;
                if (existingDetails.TryGetValue(musicId, out var detail))
                {
                    detail.ScoreUi2 = 1;
                    detail.ScoreUi6 = unplayedFlag;
                    continue;
                }

                CardDbContext.CardDetails.Add(new CardDetail
                {
                    CardId = request.CardId,
                    Pcol1 = 10,
                    Pcol2 = musicId,
                    Pcol3 = 0,
                    LastPlayTenpoId = "1337",
                    LastPlayTime = DateTime.Now,
                    ScoreUi2 = 1,
                    ScoreUi6 = unplayedFlag
                });
            }
        }

        await CardDbContext.SaveChangesAsync(cancellationToken);

        return new ServiceResult<bool>(true);
    }
}
