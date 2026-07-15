# Card Detail Music Flag Correction Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove the obsolete card-detail maintenance job and make Unlock All preserve music-detail data while deriving the unplayed flag from actual play records.

**Architecture:** Quartz will stop scheduling the legacy database rewrite. `UnlockAllMusicCommandHandler` will continue to own the administrative unlock operation, but it will mutate tracked compatibility rows and add only missing rows instead of bulk-upserting replacement DTOs. Focused SQLite-backed tests will exercise the real EF Core handler and a Quartz-options test will lock down job removal.

**Tech Stack:** .NET 10, C# 14, xUnit 2.9.3, EF Core 10.0.5 with SQLite, Quartz.NET 3.16.1, native Mediator 3.0.2.

## Global Constraints

- `doc/card_detail.md` is the source of truth: `score_ui2` is the per-card song-unlocked flag.
- `score_ui6` is `1` only for an available or unlocked song with no `pcol1 = 20` play record; any difficulty record makes it `0`.
- Preserve the Unlock All API, card-existence behavior, authorization, reward configuration, and lock-system bitset behavior.
- Preserve every unrelated field on an existing `pcol1 = 10`, `pcol3 = 0` row.
- Create compatibility rows only for distinct configured `RewardType.Music` target IDs that do not already have one.
- Do not change the protocol DTO, database schema, ordinary card read/write behavior, music catalog, or global `use_flag`/`new_flag`.
- Remove only `MaintainNullValuesJob`; all other Quartz jobs and triggers remain registered.
- Do not stage or modify the user's existing `.gitignore`, `MainServer/Properties/launchSettings.json`, `doc/card_bdata.md`, or `doc/card_detail.md` changes.

## File Map

- Create `Tests/XmlSerializationTests/ApplicationSchedulingTests.cs`: verifies the removed maintenance job and trigger are absent from configured Quartz options.
- Modify `Application/DependencyInjection.cs`: removes only the obsolete job and trigger registration.
- Delete `Application/Jobs/MaintainNullValuesJob.cs`: removes the obsolete null and music-flag database rewrite.
- Modify `Tests/XmlSerializationTests/XmlSerializationTests.csproj`: references `Infrastructure` so tests can construct the real SQLite EF Core contexts.
- Create `Tests/XmlSerializationTests/UnlockAllMusicCommandTests.cs`: covers played, unplayed, missing, duplicate-config, and field-preservation behavior through the real command handler.
- Modify `Application/Api/UnlockAllMusicCommand.cs`: de-duplicates configured IDs, reads play state, mutates tracked rows, and creates only missing rows.

---

### Task 1: Remove the Obsolete Scheduled Maintenance Job

**Files:**
- Create: `Tests/XmlSerializationTests/ApplicationSchedulingTests.cs`
- Modify: `Application/DependencyInjection.cs:64-74`
- Delete: `Application/Jobs/MaintainNullValuesJob.cs`

**Interfaces:**
- Consumes: `Application.DependencyInjection.AddApplication(IServiceCollection, int)` and `IOptions<QuartzOptions>`.
- Produces: Application service registration whose `QuartzOptions.JobDetails` and `QuartzOptions.Triggers` contain no `MaintainNullValuesJob` keys.

- [ ] **Step 1: Write the failing Quartz registration test**

Create `Tests/XmlSerializationTests/ApplicationSchedulingTests.cs` with:

```csharp
using Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;

namespace XmlSerializationTests;

public class ApplicationSchedulingTests
{
    [Fact]
    public void AddApplication_DoesNotScheduleObsoleteMaintainNullValuesJob()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<QuartzOptions>>().Value;

        Assert.DoesNotContain(options.JobDetails,
            job => job.Key.Name == "MaintainNullValuesJob");
        Assert.DoesNotContain(options.Triggers,
            trigger => trigger.Key.Name == "MaintainNullValuesJob-trigger");
    }
}
```

- [ ] **Step 2: Run the test and verify RED**

Run:

```powershell
dotnet test Tests\XmlSerializationTests\XmlSerializationTests.csproj -c Release --no-restore --filter "FullyQualifiedName~ApplicationSchedulingTests.AddApplication_DoesNotScheduleObsoleteMaintainNullValuesJob"
```

Expected: FAIL at `Assert.DoesNotContain` because the current Application registration includes `MaintainNullValuesJob` and its trigger.

- [ ] **Step 3: Remove the obsolete job and only its registration**

Delete `Application/Jobs/MaintainNullValuesJob.cs` in full.

In `Application/DependencyInjection.cs`, remove this complete block and no other Quartz configuration:

```csharp
q.AddJob<MaintainNullValuesJob>(options => options.WithIdentity(MaintainNullValuesJob.KEY));
q.AddTrigger(options =>
{
    options.ForJob(MaintainNullValuesJob.KEY)
        .WithIdentity("MaintainNullValuesJob-trigger")
        .StartNow()
        .WithSimpleSchedule(x =>
        {
            x.WithIntervalInHours(refreshIntervalHours).RepeatForever();
        });
});
```

The `UpdateMonthlyScoreRankJob` registration must remain immediately before the `CleanupOnlineMatchesJob` registration after this deletion.

- [ ] **Step 4: Run the focused test and verify GREEN**

Run:

```powershell
dotnet test Tests\XmlSerializationTests\XmlSerializationTests.csproj -c Release --no-restore --filter "FullyQualifiedName~ApplicationSchedulingTests.AddApplication_DoesNotScheduleObsoleteMaintainNullValuesJob"
```

Expected: PASS, with one test passed and no warnings or errors.

- [ ] **Step 5: Verify the obsolete symbol is gone**

Run:

```powershell
rg -n "MaintainNullValuesJob" Application Tests
```

Expected: no matches and exit code `1` from `rg`.

- [ ] **Step 6: Commit the independently working removal**

```powershell
git add -- Application/DependencyInjection.cs Application/Jobs/MaintainNullValuesJob.cs Tests/XmlSerializationTests/ApplicationSchedulingTests.cs
git commit -m "Remove obsolete card detail maintenance job"
```

Expected: one focused commit containing the job deletion, registration deletion, and regression test only.

---

### Task 2: Correct Unlock All Music Compatibility Flags

**Files:**
- Modify: `Tests/XmlSerializationTests/XmlSerializationTests.csproj`
- Create: `Tests/XmlSerializationTests/UnlockAllMusicCommandTests.cs`
- Modify: `Application/Api/UnlockAllMusicCommand.cs:61-77`

**Interfaces:**
- Consumes: `UnlockAllMusicCommandHandler.Handle(UnlockAllMusicCommand, CancellationToken)`, `ICardDependencyAggregate`, `GameConfig.UnlockRewards`, and tracked `CardDetail` entities.
- Produces: configured reward rows with `ScoreUi2 = 1` and `ScoreUi6 = 0` when any matching `Pcol1 = 20` row exists, otherwise `ScoreUi6 = 1`; unrelated existing fields remain unchanged.

- [ ] **Step 1: Add the Infrastructure test reference**

Add this project reference beside the existing Application reference in `Tests/XmlSerializationTests/XmlSerializationTests.csproj`:

```xml
<ProjectReference Include="..\..\Infrastructure\Infrastructure.csproj" />
```

The resulting project-reference item group must be:

```xml
<ItemGroup>
  <ProjectReference Include="..\..\Application\Application.csproj" />
  <ProjectReference Include="..\..\Infrastructure\Infrastructure.csproj" />
</ItemGroup>
```

- [ ] **Step 2: Write the failing handler regressions**

Create `Tests/XmlSerializationTests/UnlockAllMusicCommandTests.cs` with:

```csharp
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
```

- [ ] **Step 3: Run the handler tests and verify RED**

Run:

```powershell
dotnet test Tests\XmlSerializationTests\XmlSerializationTests.csproj -c Release --filter "FullyQualifiedName~UnlockAllMusicCommandTests"
```

Expected: FAIL. The played row remains `score_ui6 = 1`; existing fields are replaced with DTO defaults; and the missing-row case either creates the played row with `score_ui6 = 1` or fails on duplicate configured IDs. Confirm failures are behavioral assertions from these regressions, not compilation or setup errors.

- [ ] **Step 4: Replace full-row upsert with tracked updates and missing-row inserts**

In `Application/Api/UnlockAllMusicCommand.cs`, replace the compatibility-row projection and `UpsertRange` call with:

```csharp
var musicIds = Config.UnlockRewards
    .Where(config => config.RewardType == RewardType.Music)
    .Select(config => (long)config.TargetId)
    .Distinct()
    .ToArray();

if (musicIds.Length > 0)
{
    var existingDetails = await CardDbContext.CardDetails
        .Where(detail => detail.CardId == request.CardId &&
                         detail.Pcol1 == 10 &&
                         detail.Pcol3 == 0 &&
                         musicIds.Contains(detail.Pcol2))
        .ToDictionaryAsync(detail => detail.Pcol2, cancellationToken);
    var playedMusicIds = await CardDbContext.CardDetails
        .Where(detail => detail.CardId == request.CardId &&
                         detail.Pcol1 == 20 &&
                         musicIds.Contains(detail.Pcol2))
        .Select(detail => detail.Pcol2)
        .Distinct()
        .ToHashSetAsync(cancellationToken);

    foreach (var musicId in musicIds)
    {
        var unplayedFlag = playedMusicIds.Contains(musicId) ? 0 : 1;
        if (existingDetails.TryGetValue(musicId, out var detail))
        {
            detail.ScoreUi2 = 1;
            detail.ScoreUi6 = unplayedFlag;
            continue;
        }

        CardDbContext.CardDetails.Add(new CardDetail
        {
            CardId = request.CardId,
            Pcol1 = 10,
            Pcol2 = musicId,
            Pcol3 = 0,
            LastPlayTenpoId = "1337",
            LastPlayTime = DateTime.Now,
            ScoreUi2 = 1,
            ScoreUi6 = unplayedFlag
        });
    }
}

await CardDbContext.SaveChangesAsync(cancellationToken);
```

Keep the earlier card-existence check and lock-system bitset block byte-for-byte unless formatting requires moving whitespace. `using Application.Common.Helpers;`, `using Domain.Entities;`, and `using Domain.Enums;` all remain required.

- [ ] **Step 5: Run the handler tests and verify GREEN**

Run:

```powershell
dotnet test Tests\XmlSerializationTests\XmlSerializationTests.csproj -c Release --no-restore --filter "FullyQualifiedName~UnlockAllMusicCommandTests"
```

Expected: PASS, with all three handler tests passing and no warnings or errors.

- [ ] **Step 6: Run all focused regressions together**

Run:

```powershell
dotnet test Tests\XmlSerializationTests\XmlSerializationTests.csproj -c Release --no-restore --filter "FullyQualifiedName~UnlockAllMusicCommandTests|FullyQualifiedName~ApplicationSchedulingTests"
```

Expected: PASS, with four focused tests passing.

- [ ] **Step 7: Commit the flag correction**

```powershell
git add -- Application/Api/UnlockAllMusicCommand.cs Tests/XmlSerializationTests/XmlSerializationTests.csproj Tests/XmlSerializationTests/UnlockAllMusicCommandTests.cs
git commit -m "Fix music detail unlock flags"
```

Expected: one focused commit containing the handler behavior and its SQLite regressions only.

---

### Task 3: Full Verification and Diff Review

**Files:**
- Review only: all files changed by Tasks 1 and 2.

**Interfaces:**
- Consumes: the two independently green commits from Tasks 1 and 2.
- Produces: evidence that the complete solution builds and tests cleanly without unrelated staged files.

- [ ] **Step 1: Run the complete Release test suite**

Run:

```powershell
dotnet test GC-local-server-rewrite.sln -c Release --no-restore
```

Expected: PASS for every test project, with zero failed tests.

- [ ] **Step 2: Run the CI-equivalent Release build**

Run:

```powershell
dotnet build GC-local-server-rewrite.sln -c Release --no-restore
```

Expected: build succeeds with zero errors and zero warnings.

- [ ] **Step 3: Confirm structural removal and flag ownership**

Run:

```powershell
rg -n "MaintainNullValuesJob" Application Tests
```

Expected: no matches and exit code `1`.

Run:

```powershell
rg -n "ScoreUi2\s*=|ScoreUi6\s*=" Application -g "*.cs"
```

Expected: the only administrative synthesized music-flag assignments are the tracked-update and missing-row paths in `UnlockAllMusicCommand.cs`; ordinary mapper/generated assignments are outside this source scan.

- [ ] **Step 4: Review whitespace, scope, and working-tree ownership**

Run:

```powershell
git diff --check ebd0602..HEAD
git diff --stat ebd0602..HEAD
git status --short
```

Expected: no whitespace errors; the implementation diff contains only the two planned commits; the user's pre-existing `.gitignore`, documentation, and launch-settings changes remain unstaged. SQLite `card.db3-shm`/`card.db3-wal` files may remain ignored while the local MainServer process is running and must not be staged or deleted while in use.

- [ ] **Step 5: Review the implementation diff against the approved spec**

Run:

```powershell
git diff ebd0602..HEAD -- Application Tests/XmlSerializationTests
```

Expected: no protocol/schema changes, no unrelated refactors, no replacement of existing music-detail rows, no maintenance-job references, and no changes to the bitset block.
