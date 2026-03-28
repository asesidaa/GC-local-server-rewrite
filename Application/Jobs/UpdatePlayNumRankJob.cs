using System.Diagnostics.CodeAnalysis;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Application.Jobs;

public class UpdatePlayNumRankJob : IJob
{
    private readonly ILogger<UpdatePlayNumRankJob> logger;

    private readonly ICardDbContext cardDbContext;

    private readonly IMusicDbContext musicDbContext;
    
    public static readonly JobKey KEY = new JobKey("UpdatePlayNumRankJob");

    public UpdatePlayNumRankJob(ILogger<UpdatePlayNumRankJob> logger, ICardDbContext cardDbContext,
        IMusicDbContext musicDbContext)
    {
        this.logger = logger;
        this.cardDbContext = cardDbContext;
        this.musicDbContext = musicDbContext;
    }

    [SuppressMessage("ReSharper.DPA", "DPA0007: Large number of DB records",
        Justification = "All music will be read")]
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Start maintaining play num rank");
        var cancellationToken = context.CancellationToken;

        // Aggregate play counts per music in SQL instead of loading all records into memory
        var playCountsByMusic = await cardDbContext.CardDetails
            .Where(detail => detail.Pcol1 == 20)
            .GroupBy(detail => detail.Pcol2)
            .Select(g => new { MusicId = g.Key, PlayCount = g.Sum(d => d.ScoreUi1) })
            .ToDictionaryAsync(x => x.MusicId, x => x.PlayCount, cancellationToken);

        var musics = await musicDbContext.MusicUnlocks.ToListAsync(cancellationToken);
        var playNumRanks = musics.Select(music => new PlayNumRank
        {
            MusicId = (int)music.MusicId,
            Artist = music.Artist ?? string.Empty,
            Title = music.Title,
            PlayCount = (int)playCountsByMusic.GetValueOrDefault(music.MusicId)
        })
        .OrderByDescending(rank => rank.PlayCount)
        .Select((rank, i) =>
        {
            rank.Rank = i + 1;
            return rank;
        })
        .ToList();

        await cardDbContext.PlayNumRanks.UpsertRange(playNumRanks).RunAsync(cancellationToken);
        await cardDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Updating play num rank done");
    }
}