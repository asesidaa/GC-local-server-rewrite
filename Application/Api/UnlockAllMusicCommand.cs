using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Application.Api;

public record UnlockAllMusicCommand(long CardId) : IRequestWrapper<bool>;

public class UnlockAllMusicCommandHandler : RequestHandlerBase<UnlockAllMusicCommand, bool>
{
    private readonly ILogger<UnlockAllMusicCommandHandler> logger;

    public UnlockAllMusicCommandHandler(ICardDependencyAggregate aggregate,
        ILogger<UnlockAllMusicCommandHandler>                    logger) : base(aggregate)
    {
        this.logger = logger;
    }

    [SuppressMessage("ReSharper.DPA", "DPA0007: Large number of DB records")]
    [SuppressMessage("ReSharper.DPA", "DPA0006: Large number of DB commands")]
    public override async Task<ServiceResult<bool>> Handle(UnlockAllMusicCommand request, CancellationToken cancellationToken)
    {
        var unlocks = await CardDbContext.CardDetails.Where(
            detail => detail.CardId   == request.CardId &&
                      detail.Pcol1    == 10             &&
                      detail.ScoreUi6 == 1).ToListAsync(cancellationToken: cancellationToken);
        if (unlocks.Count == 0)
        {
            logger.LogWarning("Attempt to unlock for card {Card} that does not exist or is empty!", request.CardId);
            return ServiceResult.Failed<bool>(ServiceError.CustomMessage("Unlock failed"));
        }

        foreach (var unlock in unlocks)
        {
            unlock.ScoreUi2 = 1;
        }
        CardDbContext.CardDetails.UpdateRange(unlocks);
        await CardDbContext.SaveChangesAsync(cancellationToken);

        return new ServiceResult<bool>(true);
    }
}