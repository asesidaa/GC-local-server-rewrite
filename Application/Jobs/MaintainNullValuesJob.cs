using System.Diagnostics.CodeAnalysis;
using Domain.Config;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace Application.Jobs;

public class MaintainNullValuesJob : IJob
{
    private readonly ILogger<MaintainNullValuesJob> logger;

    private readonly ICardDbContext cardDbContext;

    private readonly GameConfig config;

    public static readonly JobKey KEY = new("MaintainNullValuesJob");

    public MaintainNullValuesJob(ILogger<MaintainNullValuesJob> logger, ICardDbContext cardDbContext, IOptions<GameConfig> options)
    {
        this.logger = logger;
        this.cardDbContext = cardDbContext;
        config = options.Value;
    }

    [SuppressMessage("ReSharper.DPA", "DPA0007: Large number of DB records", 
        Justification = "All details might be read")]
    [SuppressMessage("ReSharper.DPA", "DPA0006: Large number of DB commands")]
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Starting changing null values in card detail table");

        var details = await cardDbContext.CardDetails.Where(detail => detail.LastPlayTenpoId == null ||
                                                                      detail.LastPlayTenpoId == "GC local server"
                                                                      || detail.LastPlayTime == null).ToListAsync();
        details.ForEach(detail =>
        {
            detail.LastPlayTenpoId = "1337";
            detail.LastPlayTime = DateTime.MinValue;
        });

        cardDbContext.CardDetails.UpdateRange(details);
        var count = await cardDbContext.SaveChangesAsync(new CancellationToken());

        logger.LogInformation("Updated {Count} entries in card detail table", count);
        
        logger.LogInformation("Starting closing unfinished matches");
        var matches = await cardDbContext.OnlineMatches.Where(match => match.IsOpen == true).ToListAsync();
        matches.ForEach(match => match.IsOpen = false);
        cardDbContext.OnlineMatches.UpdateRange(matches);
        count = await cardDbContext.SaveChangesAsync(new CancellationToken());

        logger.LogInformation("Closed {Count} matches", count);
        
        logger.LogInformation("Starting to remove previously new songs");
        var unlockables = config.UnlockRewards
            .Where(c => c.RewardType == RewardType.Music).ToDictionary(rewardConfig => rewardConfig.TargetId);
        var targets = await cardDbContext.CardDetails.Where(detail => detail.Pcol1 == 10).ToListAsync();
        foreach (var target in targets)
        {
            if (unlockables.ContainsKey((int)target.Pcol2))
            {
                continue;
            }
            target.ScoreUi2 = 0;
            target.ScoreUi6 = 0;
        }
        cardDbContext.CardDetails.UpdateRange(targets);
        count = await cardDbContext.SaveChangesAsync(new CancellationToken());

        logger.LogInformation("Fixed {Count} records", count);
    }
}