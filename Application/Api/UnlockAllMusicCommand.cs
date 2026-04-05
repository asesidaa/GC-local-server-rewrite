using System.Diagnostics.CodeAnalysis;
using Application.Common.Helpers;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Api;

public record UnlockAllMusicCommand(long CardId) : IRequestWrapper<bool>;

public class UnlockAllMusicCommandHandler : RequestHandlerBase<UnlockAllMusicCommand, bool>
{
    private readonly ILogger<UnlockAllMusicCommandHandler> logger;

    public UnlockAllMusicCommandHandler(ICardDependencyAggregate aggregate,
        ILogger<UnlockAllMusicCommandHandler> logger) : base(aggregate)
    {
        this.logger = logger;
    }

    [SuppressMessage("ReSharper.DPA", "DPA0007: Large number of DB records")]
    [SuppressMessage("ReSharper.DPA", "DPA0006: Large number of DB commands")]
    public override async Task<ServiceResult<bool>> Handle(UnlockAllMusicCommand request, CancellationToken cancellationToken)
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

        // Also upsert CardDetail rows for compatibility
        var unlockables = Config.UnlockRewards
            .Where(config => config.RewardType == RewardType.Music)
            .Select(config => new CardDetailDto
            {
                CardId = request.CardId,
                Pcol1 = 10,
                Pcol2 = config.TargetId,
                Pcol3 = 0,
                LastPlayTenpoId = "1337",
                LastPlayTime = DateTime.Now,
                ScoreUi2 = 1,
                ScoreUi6 = 1
            }.DtoToCardDetail());

        await CardDbContext.CardDetails.UpsertRange(unlockables).RunAsync(cancellationToken);
        await CardDbContext.SaveChangesAsync(cancellationToken);

        return new ServiceResult<bool>(true);
    }
}
