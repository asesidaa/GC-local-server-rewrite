using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Application.Jobs;

public class MaintainNullValuesJob : IJob
{
    private readonly ILogger<MaintainNullValuesJob> logger;

    private readonly ICardDbContext cardDbContext;
    
    public static readonly JobKey KEY = new("MaintainNullValuesJob");

    public MaintainNullValuesJob(ILogger<MaintainNullValuesJob> logger, ICardDbContext cardDbContext)
    {
        this.logger = logger;
        this.cardDbContext = cardDbContext;
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
    }
}