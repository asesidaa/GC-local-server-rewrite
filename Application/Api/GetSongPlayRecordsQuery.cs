using System.Diagnostics.CodeAnalysis;
using Domain.Entities;
using Domain.Enums;

namespace Application.Api;

public record GetSongPlayRecordsQuery(long cardId) : IRequestWrapper<List<SongPlayRecord>>;

public class GetSongPlayRecordsQueryHandler : RequestHandlerBase<GetSongPlayRecordsQuery, List<SongPlayRecord>>
{
    public GetSongPlayRecordsQueryHandler(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    [SuppressMessage("ReSharper.DPA", "DPA0007: Large number of DB records")]
    public override async Task<ServiceResult<List<SongPlayRecord>>> Handle(GetSongPlayRecordsQuery request,
        CancellationToken cancellationToken)
    {
        var exists = await CardDbContext.CardDetails.AnyAsync(detail => detail.CardId == request.cardId);
        if (!exists)
        {
            return ServiceResult.Failed<List<SongPlayRecord>>(ServiceError.CustomMessage("No play record"));
        }
        var results = new List<SongPlayRecord>();

        var musics = await MusicDbContext.MusicUnlocks.ToDictionaryAsync(unlock => unlock.MusicId, cancellationToken);

        var playCounts = await CardDbContext.CardDetails
            .Where(detail => detail.CardId == request.cardId &&
                             detail.Pcol1  == 20)
            .Select(detail => new
            {
                MusicId = detail.Pcol2,
                Difficulty = (Difficulty)detail.Pcol3,
                Detail = detail
            })
            .ToDictionaryAsync(arg => new { arg.MusicId, arg.Difficulty }, cancellationToken: cancellationToken);

        var stageDetails = await CardDbContext.CardDetails
            .Where(detail => detail.CardId == request.cardId &&
                             detail.Pcol1  == 21)
            .Select(detail => new
            {
                MusicId = detail.Pcol2,
                Difficulty = (Difficulty)detail.Pcol3,
                Score = detail.ScoreUi1,
                MaxChain = detail.ScoreUi3,
            })
            .ToDictionaryAsync(arg => new { arg.MusicId, arg.Difficulty }, cancellationToken);

        var favorites = await CardDbContext.CardDetails
            .Where(detail => detail.CardId == request.cardId &&
                             detail.Pcol1  == 10)
            .Select(detail => new { MusicId = detail.Pcol2, IsFavorite = detail.Fcol1 == 1 })
            .ToListAsync(cancellationToken);

        foreach (var song in favorites)
        {
            var musicId = song.MusicId;
            var music = musics.GetValueOrDefault(musicId);
            var songPlayRecord = new SongPlayRecord
            {
                MusicId = (int)musicId,
                Title = music?.Title   ?? string.Empty,
                Artist = music?.Artist ?? string.Empty,
                IsFavorite = song.IsFavorite
            };
            foreach (var difficulty in DifficultyExtensions.GetValues())
            {
                var key = new { MusicId = musicId, Difficulty = difficulty };
                if (!playCounts.ContainsKey(key) || !stageDetails.ContainsKey(key)) continue;
                var playCountDetail = playCounts[key].Detail;
                var playCount = playCountDetail.ScoreUi1;
                var stageDetail = stageDetails[key];
                var score = stageDetail.Score;
                var maxChain = stageDetail.MaxChain;
                var clearState = GetClearState(playCountDetail);
                var stagePlayRecord = new StagePlayRecord
                {
                    Difficulty = difficulty,
                    ClearState = clearState,
                    PlayCount = (int)playCount,
                    Score = (int)score,
                    MaxChain = (int)maxChain,
                    LastPlayTime = playCountDetail?.LastPlayTime ?? DateTime.MinValue
                };
                songPlayRecord.StagePlayRecords.Add(stagePlayRecord);
            }

            songPlayRecord.TotalPlayCount = songPlayRecord.StagePlayRecords.Sum(record => record.PlayCount);
            if (songPlayRecord.StagePlayRecords.Count > 0)
            {
                results.Add(songPlayRecord);
            }
        }

        return new ServiceResult<List<SongPlayRecord>>(results);
    }

    private static ClearState GetClearState(CardDetail detail)
    {
        var result = ClearState.Failed;
        if (detail.ScoreUi2 > 0)
        {
            result = ClearState.Clear;
        }

        if (detail.ScoreUi3 > 0)
        {
            result = ClearState.NoMiss;
        }

        if (detail.ScoreUi4 > 0)
        {
            result = ClearState.FullChain;
        }

        if (detail.ScoreUi6 > 0)
        {
            result = ClearState.Perfect;
        }

        return result;
    }
}