using Microsoft.Extensions.Logging;
using Quartz;

namespace Application.Jobs;

public class CleanupOnlineMatchesJob : IJob
{
    public static readonly JobKey KEY = new("CleanupOnlineMatchesJob");

    private readonly ILogger<CleanupOnlineMatchesJob> logger;

    private readonly IOnlineMatchingService matchingService;

    public CleanupOnlineMatchesJob(ILogger<CleanupOnlineMatchesJob> logger, IOnlineMatchingService matchingService)
    {
        this.logger = logger;
        this.matchingService = matchingService;
    }

    public Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Starting cleanup of stale online match rooms");
        matchingService.CleanupStaleRooms();
        return Task.CompletedTask;
    }
}
