using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Game.Option;

public record PlayCountQuery(long CardId) : IRequest<long>;

public class PlayCountQueryHandler : IRequestHandler<PlayCountQuery, long>
{
    private readonly ICardDbContext context;

    private readonly ILogger<PlayCountQueryHandler> logger;

    public PlayCountQueryHandler(ICardDbContext context, ILogger<PlayCountQueryHandler> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    public async Task<long> Handle(PlayCountQuery request, CancellationToken cancellationToken)
    {
        return await GetPlayCount(request.CardId);
    }

    private async Task<long> GetPlayCount(long cardId)
    {
        var record = await context.CardPlayCounts.FirstOrDefaultAsync(count => count.CardId == cardId);
        if (record is null)
        {
            return 0;
        }
        
        var now = DateTime.Now;
        var lastPlayedTime = record.LastPlayedTime;

        if (now <= lastPlayedTime)
        {
            logger.LogWarning("Clock skew detected! " +
                              "Current time: {Now}," +
                              "Last Play Time: {Last}", now, lastPlayedTime);
            return 0;
        }

        DateTime start;
        DateTime end;
        if (now.Hour >= 8)
        {
            start = DateTime.Today.AddHours(8);
            end = start.AddHours(24);
        }
        else
        {
            end = DateTime.Today.AddHours(8);
            start = end.AddHours(-24);
        }

        var inBetween = lastPlayedTime >= start && lastPlayedTime <= end;
        return inBetween ? record.PlayCount : 0;
    }
}