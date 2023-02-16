using Domain.Entities;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Application.Jobs;

public class UpdateMonthlyScoreRankJob : IJob
{
    private readonly ILogger<UpdateMonthlyScoreRankJob> logger;

    private readonly ICardDbContext cardDbContext;
    
    public static readonly JobKey KEY = new JobKey("UpdateMonthlyScoreRankJob");

    public UpdateMonthlyScoreRankJob(ILogger<UpdateMonthlyScoreRankJob> logger, ICardDbContext cardDbContext)
    {
        this.logger = logger;
        this.cardDbContext = cardDbContext;
    }

    public async Task Execute(IJobExecutionContext context)
     {
        logger.LogInformation("Starting update montly global rank");

        var cardMains = await cardDbContext.CardMains.ToListAsync();

        var totalScoresByCardId = await cardDbContext.CardDetails.Where(detail => detail.Pcol1 == 21 && detail.LastPlayTime >= DateTime.Today.AddDays(-30))
            .GroupBy(detail => detail.CardId)
            .Select(detailGroup => new
            {
                CardId = detailGroup.Key,
                TotalScore = detailGroup.Sum(detail => detail.ScoreUi1)
            })
            .ToListAsync();

        var avatarAndTitles = await cardDbContext.CardDetails.Where(detail => detail.Pcol1 == 0 &&
                                                                        detail.Pcol2 == 0 &&
                                                                        detail.Pcol3 == 0).ToListAsync();

        var ranks = new List<MonthlyScoreRank>();
        foreach (var record in totalScoresByCardId)
        {
            var cardId = record.CardId;
            var score = record.TotalScore;
            var card = cardMains.FirstOrDefault(card => card.CardId == cardId);
            if (card is null)
            {
                logger.LogWarning("Card id {CardId} missing in main card table!", cardId);
                continue;
            }

            var detail = avatarAndTitles.First(detail => detail.CardId == cardId);

            var globalRank = new MonthlyScoreRank
            {
                CardId = cardId,
                PlayerName = card.PlayerName,
                Fcol1 = detail.Fcol1,
                Area = "Local",
                AreaId = 1,
                Pref = "nesys",
                PrefId = 1337,
                LastPlayTenpoId = 1337,
                TenpoName = "GCLocalServer",
                AvatarId = (int)detail.ScoreI1,
                Title = "Title",
                TitleId = detail.Fcol2,
                TotalScore = score
            };
            
            ranks.Add(globalRank);
        }

        ranks.AddRange(GetFakeRanks());
        ranks.Sort((rank, other) => -rank.TotalScore.CompareTo(other.TotalScore));
        ranks = ranks.Select((rank, i) =>
        {
            rank.Rank = i + 1;
            return rank;
        }).ToList();
        
        await cardDbContext.MonthlyScoreRanks.UpsertRange(ranks).RunAsync();
        await cardDbContext.SaveChangesAsync(new CancellationToken());
        
        logger.LogInformation("Updating monthly score rank done");
    }

    private static IEnumerable<MonthlyScoreRank> GetFakeRanks()
    {
        var fakeList = new List<MonthlyScoreRank>();
        for (int i = 0; i < 5; i++)
        {
            var rank = new MonthlyScoreRank
            {
                CardId = 1020392010281502 + i,
                PlayerName = $"Fake{i}",
                Fcol1 = 0,
                Area = "Local",
                AreaId = 1,
                Pref = "nesys",
                PrefId = 1337,
                LastPlayTenpoId = 1337,
                TenpoName = "GCLocalServer",
                AvatarId = i + 10,
                Title = "Title",
                TitleId = i + 100,
                TotalScore = (i + 1) * 1000000
            };
            
            fakeList.Add(rank);
        }

        return fakeList;
    }
}