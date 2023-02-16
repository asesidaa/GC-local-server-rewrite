using System.Diagnostics.CodeAnalysis;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
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

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Start maintaining play num rank");
        await UpdatePlayNumRank();
    }


    [SuppressMessage("ReSharper.DPA", "DPA0007: Large number of DB records", 
        Justification = "All music will be read")]
    private async Task UpdatePlayNumRank()
    {
        var playRecords = await cardDbContext.CardDetails
            .Where(detail => detail.Pcol1 == 20).ToListAsync();

        var playNumRanks = new List<PlayNumRank>();
        var musics = await musicDbContext.MusicUnlocks.ToListAsync();
        foreach (var music in musics)
        {
            var playCount = playRecords
                .Where(detail => detail.Pcol2 == music.MusicId)
                .Sum(detail => detail.ScoreUi1);
            var playNumRank = new PlayNumRank
            {
                MusicId = (int)music.MusicId,
                Artist = music.Artist ?? string.Empty,
                Title = music.Title,
                PlayCount = (int)playCount
            };
            playNumRanks.Add(playNumRank);
        }
        playNumRanks = playNumRanks.OrderByDescending(rank => rank.PlayCount).ToList();
        var result = playNumRanks.Select((rank, i) =>
        { 
            rank.Rank = i+1;
            return rank;
        }).ToList();
        await cardDbContext.PlayNumRanks.UpsertRange(result).RunAsync();
        await cardDbContext.SaveChangesAsync(new CancellationToken());
        
        logger.LogInformation("Updating play num rank done");
    }
}