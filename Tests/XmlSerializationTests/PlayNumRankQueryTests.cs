using System.Xml.Linq;
using Application.Game.Rank;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace XmlSerializationTests;

public class PlayNumRankQueryTests
{
    [Fact]
    public async Task Handle_FewerThanThirtyPlayedSongs_AppendsAllUnplayedCandidates()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        var options = new DbContextOptionsBuilder<CardDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var cards = new CardDbContext(options);
        await cards.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        cards.PlayNumRanks.AddRange(Enumerable.Range(1, 40).Select(musicId => new PlayNumRank
        {
            MusicId = musicId,
            PlayCount = musicId switch
            {
                10 => 20,
                20 => 10,
                _ => 0
            },
            Rank = musicId switch
            {
                10 => 1,
                20 => 2,
                _ => musicId + 2
            },
            Title = $"Song {musicId}",
            Artist = $"Artist {musicId}"
        }));
        await cards.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetPlayNumRankQueryHandler(cards);
        var result = await handler.Handle(
            new GetPlayNumRankQuery(),
            TestContext.Current.CancellationToken);

        Assert.True(result.Succeeded);
        var document = XDocument.Parse(Assert.IsType<string>(result.Data));
        var records = document.Root?
            .Element("play_num_rank")?
            .Elements("record")
            .ToList();
        Assert.NotNull(records);
        Assert.Equal(40, records.Count);
        Assert.Equal(["10", "20"], records.Take(2).Select(GetMusicId));
        Assert.Equal(40, records.Select(GetMusicId).Distinct().Count());
        Assert.Equal(
            Enumerable.Range(1, 40),
            records.Select(record => int.Parse(record.Element("rank")!.Value)));
        Assert.Equal(
            "40",
            document.Root?.Element("ranking_status")?.Element("rows")?.Value);
    }

    private static string GetMusicId(XElement record)
    {
        return record.Element("pcol1")?.Value
            ?? throw new InvalidOperationException("Play number rank record has no music id.");
    }
}
