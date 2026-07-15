using Application.Api;
using Application.Game.Card;
using Domain.Config;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace XmlSerializationTests;

public class UnlockAllMusicCommandTests
{
    private const long CardId = 42;

    [Fact]
    public async Task Handle_ExistingPlayedReward_PreservesDetailAndMarksPlayed()
    {
        await using var database = await TestDatabase.CreateAsync();
        var lastPlayTime = new DateTime(2026, 7, 15, 12, 30, 0);
        database.Cards.CardDetails.AddRange(
            new CardDetail
            {
                CardId = CardId,
                Pcol1 = 10,
                Pcol2 = 101,
                Pcol3 = 0,
                ScoreI1 = 17,
                ScoreUi2 = 0,
                ScoreUi6 = 1,
                Fcol1 = 1,
                LastPlayTenpoId = "9001",
                LastPlayTime = lastPlayTime
            },
            new CardDetail
            {
                CardId = CardId,
                Pcol1 = 20,
                Pcol2 = 101,
                Pcol3 = 2,
                LastPlayTenpoId = "9001",
                LastPlayTime = lastPlayTime
            });
        await database.Cards.SaveChangesAsync();

        var handler = CreateHandler(database, 101);
        var result = await handler.Handle(new UnlockAllMusicCommand(CardId), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.True(result.Data);
        database.Cards.ChangeTracker.Clear();
        var detail = await database.Cards.CardDetails.SingleAsync(row =>
            row.CardId == CardId && row.Pcol1 == 10 && row.Pcol2 == 101 && row.Pcol3 == 0);
        Assert.Equal(1, detail.ScoreUi2);
        Assert.Equal(0, detail.ScoreUi6);
        Assert.Equal(17, detail.ScoreI1);
        Assert.Equal(1, detail.Fcol1);
        Assert.Equal("9001", detail.LastPlayTenpoId);
        Assert.Equal(lastPlayTime, detail.LastPlayTime);
    }

    [Fact]
    public async Task Handle_ExistingUnplayedReward_PreservesDetailAndMarksUnplayed()
    {
        await using var database = await TestDatabase.CreateAsync();
        database.Cards.CardDetails.Add(new CardDetail
        {
            CardId = CardId,
            Pcol1 = 10,
            Pcol2 = 202,
            Pcol3 = 0,
            ScoreI1 = 9,
            ScoreUi2 = 0,
            ScoreUi6 = 0,
            Fcol1 = 1,
            LastPlayTenpoId = "8123",
            LastPlayTime = new DateTime(2026, 7, 14, 8, 0, 0)
        });
        await database.Cards.SaveChangesAsync();

        var handler = CreateHandler(database, 202);
        var result = await handler.Handle(new UnlockAllMusicCommand(CardId), CancellationToken.None);

        Assert.True(result.Succeeded);
        database.Cards.ChangeTracker.Clear();
        var detail = await database.Cards.CardDetails.SingleAsync(row =>
            row.CardId == CardId && row.Pcol1 == 10 && row.Pcol2 == 202 && row.Pcol3 == 0);
        Assert.Equal(1, detail.ScoreUi2);
        Assert.Equal(1, detail.ScoreUi6);
        Assert.Equal(9, detail.ScoreI1);
        Assert.Equal(1, detail.Fcol1);
        Assert.Equal("8123", detail.LastPlayTenpoId);
    }

    [Fact]
    public async Task Handle_MissingRewardDetails_CreatesDistinctRowsWithPlayDerivedFlags()
    {
        await using var database = await TestDatabase.CreateAsync();
        database.Cards.CardDetails.AddRange(
            new CardDetail
            {
                CardId = CardId,
                Pcol1 = 0,
                Pcol2 = 0,
                Pcol3 = 0,
                LastPlayTenpoId = "1337",
                LastPlayTime = DateTime.MinValue
            },
            new CardDetail
            {
                CardId = CardId,
                Pcol1 = 20,
                Pcol2 = 303,
                Pcol3 = 3,
                LastPlayTenpoId = "1337",
                LastPlayTime = DateTime.Now
            });
        await database.Cards.SaveChangesAsync();

        var handler = CreateHandler(database, 303, 303, 404);
        var result = await handler.Handle(new UnlockAllMusicCommand(CardId), CancellationToken.None);

        Assert.True(result.Succeeded);
        database.Cards.ChangeTracker.Clear();
        var details = await database.Cards.CardDetails
            .Where(row => row.CardId == CardId && row.Pcol1 == 10)
            .OrderBy(row => row.Pcol2)
            .ToListAsync();
        Assert.Collection(details,
            played =>
            {
                Assert.Equal(303, played.Pcol2);
                Assert.Equal(1, played.ScoreUi2);
                Assert.Equal(0, played.ScoreUi6);
            },
            unplayed =>
            {
                Assert.Equal(404, unplayed.Pcol2);
                Assert.Equal(1, unplayed.ScoreUi2);
                Assert.Equal(1, unplayed.ScoreUi6);
            });
    }

    private static UnlockAllMusicCommandHandler CreateHandler(TestDatabase database,
        params int[] rewardMusicIds)
    {
        var rewards = rewardMusicIds.Select((musicId, index) => new UnlockRewardConfig
        {
            RewardId = index + 1,
            RewardType = RewardType.Music,
            TargetId = musicId,
            TargetNum = 1,
            KeyNum = index + 1
        }).ToList();
        var aggregate = new CardDependencyAggregate(
            database.Cards,
            database.Music,
            Options.Create(new GameConfig { UnlockRewards = rewards }),
            Options.Create(new UnlockConfig { EnableLockSystem = false }));
        return new UnlockAllMusicCommandHandler(
            aggregate,
            NullLogger<UnlockAllMusicCommandHandler>.Instance);
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection cardConnection;

        private TestDatabase(SqliteConnection cardConnection, CardDbContext cards,
            MusicDbContext music)
        {
            this.cardConnection = cardConnection;
            Cards = cards;
            Music = music;
        }

        public CardDbContext Cards { get; }

        public MusicDbContext Music { get; }

        public static async Task<TestDatabase> CreateAsync()
        {
            var cardConnection = new SqliteConnection("Data Source=:memory:");
            await cardConnection.OpenAsync();
            var cardOptions = new DbContextOptionsBuilder<CardDbContext>()
                .UseSqlite(cardConnection)
                .Options;
            var cards = new CardDbContext(cardOptions);
            await cards.Database.EnsureCreatedAsync();

            var musicOptions = new DbContextOptionsBuilder<MusicDbContext>()
                .UseSqlite("Data Source=:memory:")
                .Options;
            var music = new MusicDbContext(musicOptions);
            return new TestDatabase(cardConnection, cards, music);
        }

        public async ValueTask DisposeAsync()
        {
            await Cards.DisposeAsync();
            await Music.DisposeAsync();
            await cardConnection.DisposeAsync();
        }
    }
}
