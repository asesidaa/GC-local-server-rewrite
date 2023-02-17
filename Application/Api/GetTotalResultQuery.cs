using Microsoft.Extensions.Logging;
using Shared.Models;

namespace Application.Api;

public record GetTotalResultQuery(long CardId) : IRequestWrapper<TotalResultData>;

public class GetTotalResultQueryHandler : RequestHandlerBase<GetTotalResultQuery, TotalResultData>
{
    private const int S_SCORE = 900000;
    private const int SS_SCORE = 950000;
    private const int SSS_SCORE = 990000;
    
    private readonly ILogger<GetTotalResultQueryHandler> logger;
    
    public GetTotalResultQueryHandler(ICardDependencyAggregate aggregate, ILogger<GetTotalResultQueryHandler> logger) : base(aggregate)
    {
        this.logger = logger;
    }

    public override async Task<ServiceResult<TotalResultData>> Handle(GetTotalResultQuery request, CancellationToken cancellationToken)
    {
        var card = await CardDbContext.CardMains.FirstOrDefaultAsync(card => card.CardId == request.CardId, cancellationToken);
        if (card is null)
        {
            logger.LogWarning("Trying to get total result for non existing card: {CardId}", request.CardId);
            return ServiceResult.Failed<TotalResultData>(ServiceError.UserNotFound);
        }

        var result = new TotalResultData
        {
            CardId = card.CardId,
            PlayerName = card.PlayerName
        };

        var totalSongCount = await MusicDbContext.MusicUnlocks.CountAsync(cancellationToken: cancellationToken);
        var totalExtraCount = await MusicDbContext.MusicExtras.CountAsync(cancellationToken: cancellationToken);
        var totalStageCount = totalSongCount * 3 + totalExtraCount;
        result.PlayerData.TotalSongCount = totalSongCount;
        result.StageCountData.Total = totalStageCount;
        
        var playedStageDetails = await CardDbContext.CardDetails.Where(detail =>
            detail.CardId == request.CardId &&
            detail.Pcol1  == 20).ToListAsync(cancellationToken: cancellationToken);
        
        var playedStageScores = await CardDbContext.CardDetails.Where(detail =>
            detail.CardId == request.CardId &&
            detail.Pcol1  == 21).ToListAsync(cancellationToken: cancellationToken);

        var playedSongCount = playedStageDetails.DistinctBy(detail => detail.Pcol2).Count();
        var playedStageCount = playedStageDetails.Count;
        var clearedStageCount = playedStageDetails.Count(detail => detail.ScoreUi2   > 0);
        var noMissStageCount = playedStageDetails.Count(detail => detail.ScoreUi3    > 0);
        var fullChainStageCount = playedStageDetails.Count(detail => detail.ScoreUi4 > 0);
        var perfectStageCount = playedStageDetails.Count(detail => detail.ScoreUi6         > 0);

        var sStageCount = playedStageScores.Count(detail => detail.ScoreUi1 > S_SCORE);
        var ssStageCount = playedStageScores.Count(detail => detail.ScoreUi1 > SS_SCORE);
        var sssStageCount = playedStageScores.Count(detail => detail.ScoreUi1 > SSS_SCORE);
        result.PlayerData.PlayedSongCount = playedSongCount;
        result.StageCountData.Cleared = clearedStageCount;
        result.StageCountData.NoMiss = noMissStageCount;
        result.StageCountData.FullChain = fullChainStageCount;
        result.StageCountData.Perfect = perfectStageCount;
        result.StageCountData.S = sStageCount;
        result.StageCountData.Ss = ssStageCount;
        result.StageCountData.Sss = sssStageCount;

        var totalScore = playedStageScores.Sum(detail => detail.ScoreUi1);
        var averageScore = playedStageCount == 0 ? 0 : totalScore / playedStageCount;
        result.PlayerData.TotalScore = totalScore;
        result.PlayerData.AverageScore = (int)averageScore;

        var rank = await CardDbContext.GlobalScoreRanks.FirstOrDefaultAsync(rank => rank.CardId == request.CardId, 
            cancellationToken: cancellationToken);
        result.PlayerData.Rank = (int)(rank?.Rank ?? -1);

        return new ServiceResult<TotalResultData>(result);
    }
}