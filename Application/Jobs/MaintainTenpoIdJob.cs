using System.Diagnostics.CodeAnalysis;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Application.Jobs;

public class MaintainTenpoIdJob : IJob
{
    private readonly ILogger<MaintainTenpoIdJob> logger;

    private readonly ICardDbContext cardDbContext;
    
    public static readonly JobKey KEY = new("MaintainTenpoIdJob");

    public MaintainTenpoIdJob(ILogger<MaintainTenpoIdJob> logger, ICardDbContext cardDbContext)
    {
        this.logger = logger;
        this.cardDbContext = cardDbContext;
    }

    [SuppressMessage("ReSharper.DPA", "DPA0007: Large number of DB records", 
        Justification = "All details might be read")]
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Starting changing null values in card detail table");

        var details = await cardDbContext.CardDetails.Where(detail => detail.LastPlayTenpoId == null).ToListAsync();
        details.ForEach(detail => detail.LastPlayTenpoId="1337");

        cardDbContext.CardDetails.UpdateRange(details);
        var count = await cardDbContext.SaveChangesAsync(new CancellationToken());

        logger.LogInformation("Updated {Count} entries in card detail table", count);
    }
}