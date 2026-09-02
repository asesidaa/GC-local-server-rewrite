using System.Xml.Linq;
using Application.Common.Extensions;
using Application.Game.Rank;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace XmlSerializationTests;

public class WorldScoreRankQueryTests
{
    [Fact]
    public async Task Handle_PersonalWorldScore_EmitsWorldRoot()
    {
        await using var database = await TestDatabase.CreateAsync();
        database.Cards.GlobalScoreRanks.Add(CreateRank(1001, 1, 500_000, 10));
        await database.Cards.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetWScoreRankQueryHandler(database.Cards);
        var result = await handler.Handle(
            new GetWScoreRankQuery(CreateParam(cardId: 1001)),
            TestContext.Current.CancellationToken);

        var root = ParseRoot(result);
        Assert.NotNull(root.Element("w_score_rank"));
        Assert.Null(root.Element("score_rank"));
        Assert.Equal("1001", root.Element("w_score_rank")?.Element("record")?.Element("card_id")?.Value);
    }

    [Fact]
    public async Task Handle_WorldShopFilter_ReturnsOnlyMatchingTenpo()
    {
        await using var database = await TestDatabase.CreateAsync();
        database.Cards.GlobalScoreRanks.AddRange(
            CreateRank(1001, 1, 500_000, 10),
            CreateRank(1002, 2, 400_000, 20));
        await database.Cards.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetWScoreRankQueryHandler(database.Cards);
        var result = await handler.Handle(
            new GetWScoreRankQuery(CreateParam(tenpoId: 20)),
            TestContext.Current.CancellationToken);

        var records = ParseRoot(result).Element("w_score_rank")!.Elements("record").ToList();
        Assert.Single(records);
        Assert.Equal("1002", records[0].Element("card_id")?.Value);
    }

    [Fact]
    public async Task Handle_WorldGlobal_ReturnsAllTenpos()
    {
        await using var database = await TestDatabase.CreateAsync();
        database.Cards.GlobalScoreRanks.AddRange(
            CreateRank(1001, 1, 500_000, 10),
            CreateRank(1002, 2, 400_000, 20));
        await database.Cards.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetWScoreRankQueryHandler(database.Cards);
        var result = await handler.Handle(
            new GetWScoreRankQuery(CreateParam()),
            TestContext.Current.CancellationToken);

        var records = ParseRoot(result).Element("w_score_rank")!.Elements("record").ToList();
        Assert.Equal(2, records.Count);
    }

    [Fact]
    public async Task Handle_PersonalWorldShopScore_EmitsWorldTenpoRoot()
    {
        await using var database = await TestDatabase.CreateAsync();
        database.Cards.GlobalScoreRanks.Add(CreateRank(1001, 1, 500_000, 20));
        await database.Cards.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetWTenpoScoreRankQueryHandler(database.Cards);
        var result = await handler.Handle(
            new GetWTenpoScoreRankQuery(20, CreateParam(cardId: 1001)),
            TestContext.Current.CancellationToken);

        var root = ParseRoot(result);
        Assert.NotNull(root.Element("w_t_score_rank"));
        Assert.Null(root.Element("t_score_rank"));
        Assert.Equal("1001", root.Element("w_t_score_rank")?.Element("record")?.Element("card_id")?.Value);
    }

    private static string CreateParam(long cardId = 0, int tenpoId = 0)
    {
        return new RankParam
        {
            CardId = cardId,
            TenpoId = tenpoId
        }.SerializeCardData("/root/data");
    }

    private static XElement ParseRoot(ServiceResult<string> result)
    {
        Assert.True(result.Succeeded);
        return XDocument.Parse(Assert.IsType<string>(result.Data)).Root
            ?? throw new InvalidOperationException("Ranking response has no root element.");
    }

    private static GlobalScoreRank CreateRank(long cardId, long rank, long score, int tenpoId)
    {
        return new GlobalScoreRank
        {
            CardId = cardId,
            PlayerName = $"Player {cardId}",
            Rank = rank,
            TotalScore = score,
            LastPlayTenpoId = tenpoId,
            TenpoName = $"Shop {tenpoId}"
        };
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection connection;

        private TestDatabase(SqliteConnection connection, CardDbContext cards)
        {
            this.connection = connection;
            Cards = cards;
        }

        public CardDbContext Cards { get; }

        public static async Task<TestDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync(TestContext.Current.CancellationToken);
            var options = new DbContextOptionsBuilder<CardDbContext>()
                .UseSqlite(connection)
                .Options;
            var cards = new CardDbContext(options);
            await cards.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
            return new TestDatabase(connection, cards);
        }

        public async ValueTask DisposeAsync()
        {
            await Cards.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
