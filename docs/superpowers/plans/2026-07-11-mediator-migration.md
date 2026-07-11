# Native Mediator Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace MediatR with martinothamar/Mediator 3.0.2 using native command/query contracts, source-generated registration, and no compatibility layer while preserving all server behavior.

**Architecture:** `Application` continues to own messages, handlers, pipeline behaviors, and its DI entry point. Project response wrappers split into native query and command contracts, card-dependent handlers split over a shared dependency base, and generated Mediator registration uses scoped lifetime plus explicit logging and exception behaviors. Controllers continue dispatching through `ISender`; HTTP routes, protocol payloads, persistence behavior, and business logic do not change.

**Tech Stack:** .NET 10, C# 14, ASP.NET Core controllers, EF Core 10, Quartz, xUnit 2.9.3, Mediator.Abstractions 3.0.2, Mediator.SourceGenerator 3.0.2

## Global Constraints

- Keep `TargetFramework` at `net10.0` and `LangVersion` at `14` where already configured.
- Pin both Mediator packages to stable version `3.0.2`; do not use the 3.1 prerelease line.
- Remove the `MediatR` package and every `MediatR` namespace from active source.
- Do not add a compatibility package, alias, adapter, shim, dual registration, or request type implementing both libraries.
- Use native `IQuery<T>`/`IQueryHandler<TQuery, T>` and `ICommand<T>`/`ICommandHandler<TCommand, T>` contracts.
- Configure generated Mediator services with `ServiceLifetime.Scoped`; never accept its singleton default because handlers consume scoped EF Core services.
- Handler interface methods return `ValueTask<T>`; internal EF/helper methods may remain `Task<T>`.
- Preserve all HTTP routes, DTOs, XML output, database schemas, controller result handling, logging messages, and exception behavior.
- Do not introduce notifications, streams, telemetry, Native AOT, controller constructor refactors, or unrelated cleanup.
- Preserve the pre-existing working-tree changes in `CLAUDE.md`, `.claude/`, and `AGENTS.md`; never stage unrelated content.

---

## File Map

- `Application/Application.csproj`: owns the runtime abstractions package and the private source-generator analyzer.
- `Application/Interfaces/IQueryWrapper.cs`: project query response convention over native Mediator.
- `Application/Interfaces/ICommandWrapper.cs`: project command response convention over native Mediator.
- `Application/Common/Base/RequestHandlerBase.cs`: shared card database, music database, and configuration dependencies/helpers only.
- `Application/Common/Base/QueryHandlerBase.cs`: native query-handler base for card-dependent queries.
- `Application/Common/Base/CommandHandlerBase.cs`: native command-handler base for card-dependent commands.
- `Application/Common/Behaviours/LoggingBehaviour.cs`: native message preprocessor.
- `Application/Common/Behaviours/UnhandledExceptionBehaviour.cs`: native exception-logging pipeline behavior.
- `Application/DependencyInjection.cs`: generated registration, assembly selection, scoped lifetime, pipeline ordering, and existing Quartz setup.
- `Application/Api/**` and `Application/Game/**`: native query/command declarations and `ValueTask` handler boundaries.
- `MainServer/Controllers/BaseController.cs`: native `Mediator.ISender` resolution.
- `Tests/XmlSerializationTests/MediatorMigrationTests.cs`: contract, dispatch, and pipeline registration coverage.
- `AGENTS.md` and `CLAUDE.md`: active contributor/agent terminology after migration.

### Task 1: Add Native Command and Query Contract Foundations

**Files:**
- Modify: `Application/Application.csproj:23`
- Create: `Application/Interfaces/IQueryWrapper.cs`
- Create: `Application/Interfaces/ICommandWrapper.cs`
- Create: `Tests/XmlSerializationTests/MediatorMigrationTests.cs`

**Interfaces:**
- Consumes: `Shared.Models.ServiceResult<T>` and Mediator 3.0.2 abstractions.
- Produces: `IQueryWrapper<T>`, `IQueryHandlerWrapper<TQuery, T>`, `ICommandWrapper<T>`, and `ICommandHandlerWrapper<TCommand, T>` for Task 2.

- [ ] **Step 1: Write failing native-wrapper contract tests**

Create `Tests/XmlSerializationTests/MediatorMigrationTests.cs`:

```csharp
using Application.Interfaces;
using Mediator;
using Shared.Models;

namespace XmlSerializationTests;

public class MediatorMigrationTests
{
    [Fact]
    public void QueryWrapper_UsesNativeQueryContract()
    {
        Assert.Contains(
            typeof(IQuery<ServiceResult<int>>),
            typeof(IQueryWrapper<int>).GetInterfaces());
    }

    [Fact]
    public void CommandWrapper_UsesNativeCommandContract()
    {
        Assert.Contains(
            typeof(ICommand<ServiceResult<int>>),
            typeof(ICommandWrapper<int>).GetInterfaces());
    }
}
```

- [ ] **Step 2: Run the focused tests to verify the red state**

Run:

```powershell
dotnet test Tests/XmlSerializationTests/XmlSerializationTests.csproj -c Release --filter "FullyQualifiedName~MediatorMigrationTests"
```

Expected: compilation fails because `Mediator`, `IQueryWrapper`, and `ICommandWrapper` do not exist yet.

- [ ] **Step 3: Add the native abstractions package without changing runtime registration**

Replace the current MediatR package line in `Application/Application.csproj` with both runtime abstraction references temporarily; keep MediatR only until Task 2 performs the atomic runtime switch:

```xml
<PackageReference Include="MediatR" Version="14.1.0" />
<PackageReference Include="Mediator.Abstractions" Version="3.0.2" />
```

Do not add `Mediator.SourceGenerator` or call `AddMediator()` in this task. Runtime dispatch remains entirely on MediatR until Task 2.

- [ ] **Step 4: Implement the native query wrappers**

Create `Application/Interfaces/IQueryWrapper.cs`:

```csharp
using Mediator;
using Shared.Models;

namespace Application.Interfaces;

public interface IQueryWrapper<T> : IQuery<ServiceResult<T>>;

public interface IQueryHandlerWrapper<TQuery, T> : IQueryHandler<TQuery, ServiceResult<T>>
    where TQuery : IQueryWrapper<T>;
```

- [ ] **Step 5: Implement the native command wrappers**

Create `Application/Interfaces/ICommandWrapper.cs`:

```csharp
using Mediator;
using Shared.Models;

namespace Application.Interfaces;

public interface ICommandWrapper<T> : ICommand<ServiceResult<T>>;

public interface ICommandHandlerWrapper<TCommand, T> : ICommandHandler<TCommand, ServiceResult<T>>
    where TCommand : ICommandWrapper<T>;
```

- [ ] **Step 6: Run the focused and baseline tests**

Run:

```powershell
dotnet test Tests/XmlSerializationTests/XmlSerializationTests.csproj -c Release --filter "FullyQualifiedName~MediatorMigrationTests"
dotnet test GC-local-server-rewrite.sln -c Release
```

Expected: 2 migration tests pass; the full suite reports 16 passed, 0 failed. Existing NuGet vulnerability warnings may remain, but there must be no compiler or generator errors.

- [ ] **Step 7: Commit the contract foundation**

```powershell
git add -- Application/Application.csproj Application/Interfaces/IQueryWrapper.cs Application/Interfaces/ICommandWrapper.cs Tests/XmlSerializationTests/MediatorMigrationTests.cs
git diff --cached --check
git commit -m "Add native Mediator contract foundations"
```

Expected: only the four listed files are committed. `CLAUDE.md`, `.claude/`, and `AGENTS.md` remain untouched and unstaged.

### Task 2: Atomically Switch Dispatch, Handlers, and Pipelines

**Files:**
- Modify: `Application/Application.csproj`
- Delete: `Application/Interfaces/IRequestWrapper.cs`
- Modify: `Application/Common/Base/RequestHandlerBase.cs`
- Create: `Application/Common/Base/QueryHandlerBase.cs`
- Create: `Application/Common/Base/CommandHandlerBase.cs`
- Modify: `Application/Common/Behaviours/LoggingBehaviour.cs`
- Modify: `Application/Common/Behaviours/UnhandledExceptionBehaviour.cs`
- Modify: `Application/DependencyInjection.cs`
- Modify: `MainServer/Controllers/BaseController.cs`
- Modify: `Tests/XmlSerializationTests/MediatorMigrationTests.cs`
- Modify the 40 wrapped query files and 31 wrapped command files listed in Steps 4 and 5.
- Modify: `Application/Game/Server/GetDataQuery.cs`
- Modify: `Application/Game/Option/PlayCountQuery.cs`
- Modify: `Application/Game/Server/CertifyCommand.cs`

**Interfaces:**
- Consumes: the four wrapper contracts from Task 1 and the existing `ICardDependencyAggregate`.
- Produces: generated `AddMediator(IServiceCollection, Action<MediatorOptions>)`, scoped native `Mediator.ISender`, native query/command handlers, and ordered native pipeline behavior registrations.

- [ ] **Step 1: Expand the migration test into real contract, dispatch, and pipeline coverage**

Replace `Tests/XmlSerializationTests/MediatorMigrationTests.cs` with:

```csharp
using Application;
using Application.Api.Auth;
using Application.Common.Behaviours;
using Application.Game.Server;
using Application.Interfaces;
using Domain.Config;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Shared.Models;

namespace XmlSerializationTests;

public class MediatorMigrationTests
{
    private const string AdminPassword = "migration-test-password";

    [Fact]
    public void QueryWrapper_UsesNativeQueryContract()
    {
        Assert.Contains(
            typeof(IQuery<ServiceResult<int>>),
            typeof(IQueryWrapper<int>).GetInterfaces());
    }

    [Fact]
    public void CommandWrapper_UsesNativeCommandContract()
    {
        Assert.Contains(
            typeof(ICommand<ServiceResult<int>>),
            typeof(ICommandWrapper<int>).GetInterfaces());
    }

    [Fact]
    public async Task AddApplication_DispatchesNativeQuery()
    {
        var query = Assert.IsAssignableFrom<IQuery<ServiceResult<bool>>>(
            new ValidateAdminLoginQuery(AdminPassword));

        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(query);

        Assert.True(result.Succeeded);
        Assert.Equal(true, result.Data);
    }

    [Fact]
    public async Task AddApplication_DispatchesNativeCommand()
    {
        var command = Assert.IsAssignableFrom<ICommand<string>>(
            new CertifyCommand(null, null, null, null, "localhost"));

        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(command);

        Assert.Equal("error=0", result);
    }

    [Fact]
    public void AddApplication_RegistersOrderedNativePipelineBehaviours()
    {
        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();
        var serviceType = typeof(IPipelineBehavior<,>).MakeGenericType(
            typeof(ValidateAdminLoginQuery),
            typeof(ServiceResult<bool>));

        var behaviorTypes = scope.ServiceProvider
            .GetServices(serviceType)
            .Select(behavior => behavior.GetType().GetGenericTypeDefinition())
            .ToArray();

        Assert.Equal(
            new[] { typeof(LoggingBehaviour<,>), typeof(UnhandledExceptionBehaviour<,>) },
            behaviorTypes);
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.Configure<WebAuthConfig>(options => options.AdminPassword = AdminPassword);
        services.Configure<AuthConfig>(_ => { });
        services.Configure<RelayConfig>(_ => { });
        services.AddApplication();
        return services.BuildServiceProvider();
    }
}
```

- [ ] **Step 2: Run the focused tests to verify the migration is still red**

Run:

```powershell
dotnet test Tests/XmlSerializationTests/XmlSerializationTests.csproj -c Release --filter "FullyQualifiedName~MediatorMigrationTests"
```

Expected: the two wrapper tests pass; the query, command, and pipeline tests fail because concrete messages and runtime registration still use MediatR.

- [ ] **Step 3: Split the shared handler base by native message kind**

Replace `Application/Common/Base/RequestHandlerBase.cs` with:

```csharp
using Application.Common.Helpers;
using Domain.Config;
using Domain.Enums;

namespace Application.Common.Base;

public abstract class RequestHandlerBase
{
    protected ICardDbContext CardDbContext { get; }
    protected IMusicDbContext MusicDbContext { get; }
    protected GameConfig Config { get; }
    protected UnlockConfig UnlockConfig { get; }

    protected RequestHandlerBase(ICardDependencyAggregate aggregate)
    {
        CardDbContext = aggregate.CardDbContext;
        MusicDbContext = aggregate.MusicDbContext;
        Config = aggregate.Options.Value;
        UnlockConfig = aggregate.UnlockOptions.Value;
    }

    protected async Task<long[]?> LoadBitset(long cardId, UnlockItemType itemType, int itemCount,
        CancellationToken cancellationToken)
    {
        if (!UnlockConfig.EnableLockSystem)
            return null;

        var typeInt = (int)itemType;
        var playerState = await CardDbContext.PlayerUnlockStates
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CardId == cardId && s.ItemType == typeInt, cancellationToken);

        long[] bitset;
        if (playerState is not null)
        {
            bitset = BitsetHelper.Deserialize(playerState.UnlockedBitset);
        }
        else
        {
            var defaultState = await CardDbContext.DefaultUnlockStates
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ItemType == typeInt, cancellationToken);

            bitset = defaultState is not null
                ? BitsetHelper.Deserialize(defaultState.UnlockedBitset)
                : BitsetHelper.CreateAllZeroes(itemCount);
        }

        return BitsetHelper.EnsureLength(bitset, itemCount);
    }

    protected async Task<int> GetItemCountAsync(UnlockItemType itemType, CancellationToken cancellationToken) => itemType switch
    {
        UnlockItemType.Avatar => Config.AvatarCount,
        UnlockItemType.Navigator => Config.NavigatorCount,
        UnlockItemType.Item => Config.ItemCount,
        UnlockItemType.Skin => Config.SkinCount,
        UnlockItemType.SoundEffect => Config.SeCount,
        UnlockItemType.Title => Config.TitleCount,
        UnlockItemType.Music => await MusicDbContext.MusicUnlocks.AnyAsync(cancellationToken)
            ? await MusicDbContext.MusicUnlocks.MaxAsync(m => (int)m.MusicId, cancellationToken)
            : 0,
        _ => throw new ArgumentOutOfRangeException(nameof(itemType))
    };
}
```

Create `Application/Common/Base/QueryHandlerBase.cs`:

```csharp
namespace Application.Common.Base;

public abstract class QueryHandlerBase<TQuery, T> : RequestHandlerBase, IQueryHandlerWrapper<TQuery, T>
    where TQuery : IQueryWrapper<T>
{
    protected QueryHandlerBase(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public abstract ValueTask<ServiceResult<T>> Handle(TQuery query, CancellationToken cancellationToken);
}
```

Create `Application/Common/Base/CommandHandlerBase.cs`:

```csharp
namespace Application.Common.Base;

public abstract class CommandHandlerBase<TCommand, T> : RequestHandlerBase, ICommandHandlerWrapper<TCommand, T>
    where TCommand : ICommandWrapper<T>
{
    protected CommandHandlerBase(ICardDependencyAggregate aggregate) : base(aggregate)
    {
    }

    public abstract ValueTask<ServiceResult<T>> Handle(TCommand command, CancellationToken cancellationToken);
}
```

Keep the existing helper logic byte-for-byte equivalent; only ownership and mediator contracts change.

- [ ] **Step 4: Migrate every wrapped query contract and handler**

Apply these exact transformations to the query files listed below:

```csharp
public record GetCardsQuery() : IQueryWrapper<List<ClientCardDto>>;

public class GetCardsQueryHandler : QueryHandlerBase<GetCardsQuery, List<ClientCardDto>>

public override async ValueTask<ServiceResult<List<ClientCardDto>>> Handle(
    GetCardsQuery request,
    CancellationToken cancellationToken)

public record ValidateAdminLoginQuery(string Password) : IQueryWrapper<bool>;

public class ValidateAdminLoginQueryHandler
    : IQueryHandlerWrapper<ValidateAdminLoginQuery, bool>

public ValueTask<ServiceResult<bool>> Handle(
    ValidateAdminLoginQuery request,
    CancellationToken cancellationToken)
```

These are signature replacements only; leave each existing method body in place. For synchronous handlers, use `ValueTask<ServiceResult<T>>` and replace each `Task.FromResult(value)` in the `Handle` method with `ValueTask.FromResult(value)`. Do not change private helper methods returning `Task<T>`.

Card-dependent query files (change `IRequestWrapper` → `IQueryWrapper`, `RequestHandlerBase` → `QueryHandlerBase`, and handler `Task` → `ValueTask`):

```text
Application/Api/Auth/ValidateCardLoginQuery.cs
Application/Api/GetCardsQuery.cs
Application/Api/GetDefaultUnlockBitsetQuery.cs
Application/Api/GetDefaultUnlockStateQuery.cs
Application/Api/GetMusicListQuery.cs
Application/Api/GetPlayerCoinsQuery.cs
Application/Api/GetPlayOptionQuery.cs
Application/Api/GetShopItemsQuery.cs
Application/Api/GetSongPlayRecordsQuery.cs
Application/Api/GetTotalResultQuery.cs
Application/Api/GetUnlockStateQuery.cs
Application/Game/Card/Read/ReadAllCardDetailsQuery.cs
Application/Game/Card/Read/ReadAvatarQuery.cs
Application/Game/Card/Read/ReadCardBDataQuery.cs
Application/Game/Card/Read/ReadCardDetailQuery.cs
Application/Game/Card/Read/ReadCardQuery.cs
Application/Game/Card/Read/ReadCoinQuery.cs
Application/Game/Card/Read/ReadCondQuery.cs
Application/Game/Card/Read/ReadEventRewardQuery.cs
Application/Game/Card/Read/ReadGetMessageQuery.cs
Application/Game/Card/Read/ReadItemQuery.cs
Application/Game/Card/Read/ReadMusicAouQuery.cs
Application/Game/Card/Read/ReadMusicExtraQuery.cs
Application/Game/Card/Read/ReadMusicQuery.cs
Application/Game/Card/Read/ReadNavigatorQuery.cs
Application/Game/Card/Read/ReadSkinQuery.cs
Application/Game/Card/Read/ReadSoundEffectQuery.cs
Application/Game/Card/Read/ReadTitleQuery.cs
Application/Game/Card/Read/ReadTotalTrophyQuery.cs
Application/Game/Card/Read/ReadUnlockKeynumQuery.cs
Application/Game/Card/Read/ReadUnlockRewardQuery.cs
```

Direct wrapper-handler query files (change `IRequestWrapper` → `IQueryWrapper`, `IRequestHandlerWrapper` → `IQueryHandlerWrapper`, and handler `Task` → `ValueTask`):

```text
Application/Api/Auth/ValidateAdminLoginQuery.cs
Application/Game/Rank/GetEventRankQuery.cs
Application/Game/Rank/GetGlobalScoreRankQuery.cs
Application/Game/Rank/GetMonthlyScoreRankQuery.cs
Application/Game/Rank/GetPlayNumRankQuery.cs
Application/Game/Rank/GetTenpoScoreRankQuery.cs
Application/Game/Rank/GetWPlayNumRankQuery.cs
Application/Game/Rank/GetWScoreRankQuery.cs
Application/Game/Rank/GetWTenpoScoreRankQuery.cs
```

- [ ] **Step 5: Migrate every wrapped command contract and handler**

Apply these exact transformations to the command files listed below:

```csharp
public record AddShopItemCommand(UnlockItemType ItemType, int ItemId, int CoinCost)
    : ICommandWrapper<bool>;

public class AddShopItemCommandHandler
    : CommandHandlerBase<AddShopItemCommand, bool>

public override async ValueTask<ServiceResult<bool>> Handle(
    AddShopItemCommand request,
    CancellationToken cancellationToken)

public record StartOnlineMatchingCommand(long CardId, string Data)
    : ICommandWrapper<string>;

public class StartOnlineMatchingCommandHandler
    : ICommandHandlerWrapper<StartOnlineMatchingCommand, string>

public ValueTask<ServiceResult<string>> Handle(
    StartOnlineMatchingCommand request,
    CancellationToken cancellationToken)
```

These are signature replacements only; leave each existing method body in place. For synchronous handlers, use `ValueTask<ServiceResult<T>>` and replace each `Task.FromResult(value)` in the `Handle` method with `ValueTask.FromResult(value)`. Do not alter non-handler Quartz methods returning `Task`.

Card-dependent command files (change `IRequestWrapper` → `ICommandWrapper`, `RequestHandlerBase` → `CommandHandlerBase`, and handler `Task` → `ValueTask`):

```text
Application/Api/AddShopItemCommand.cs
Application/Api/Auth/SetCardAccessCodeCommand.cs
Application/Api/PurchaseItemCommand.cs
Application/Api/RemoveShopItemCommand.cs
Application/Api/SetDefaultUnlockCommand.cs
Application/Api/SetFavoriteMusicCommand.cs
Application/Api/SetPlayerCoinsCommand.cs
Application/Api/SetPlayerNameCommand.cs
Application/Api/SetPlayOptionCommand.cs
Application/Api/SetUnlockAllCommand.cs
Application/Api/SetUnlockCommand.cs
Application/Api/UnlockAllMusicCommand.cs
Application/Game/Card/Management/CardRegisterCommand.cs
Application/Game/Card/Management/CardReissueCommand.cs
Application/Game/Card/Session/GetSessionCommand.cs
Application/Game/Card/Write/WriteAvatarCommand.cs
Application/Game/Card/Write/WriteCardBDataCommand.cs
Application/Game/Card/Write/WriteCardCommand.cs
Application/Game/Card/Write/WriteCardDetailCommand.cs
Application/Game/Card/Write/WriteCoinCommand.cs
Application/Game/Card/Write/WriteCondCommand.cs
Application/Game/Card/Write/WriteItemCommand.cs
Application/Game/Card/Write/WriteMusicDetailCommand.cs
Application/Game/Card/Write/WriteNavigatorCommand.cs
Application/Game/Card/Write/WriteSkinCommand.cs
Application/Game/Card/Write/WriteSoundEffectCommand.cs
Application/Game/Card/Write/WriteTitleCommand.cs
Application/Game/Card/Write/WriteUnlockKeynumCommand.cs
```

Direct wrapper-handler command files (change `IRequestWrapper` → `ICommandWrapper`, `IRequestHandlerWrapper` → `ICommandHandlerWrapper`, and handler `Task` → `ValueTask`):

```text
Application/Game/Card/OnlineMatching/StartOnlineMatchingCommand.cs
Application/Game/Card/OnlineMatching/UpdateOnlineMatchingCommand.cs
Application/Game/Card/OnlineMatching/UploadOnlineMatchingResultCommand.cs
```

- [ ] **Step 6: Migrate the three direct contracts to native query/command interfaces**

Replace `Application/Game/Server/GetDataQuery.cs` with:

```csharp
using System.Text;
using Mediator;

namespace Application.Game.Server;

public record GetDataQuery(string Host, string Scheme) : IQuery<string>;

public class GetDataQueryHandler : IQueryHandler<GetDataQuery, string>
{
    private readonly IEventManagerService eventManagerService;

    public GetDataQueryHandler(IEventManagerService eventManagerService)
    {
        this.eventManagerService = eventManagerService;
    }

    public ValueTask<string> Handle(GetDataQuery request, CancellationToken cancellationToken)
    {
        var response = "count=0\n" +
                       "nexttime=180";
        if (!eventManagerService.UseEvents())
        {
            return ValueTask.FromResult(response);
        }

        var urlBase = $"{request.Scheme}://{request.Host}/events/";
        var dataString = new StringBuilder();
        var events = eventManagerService.GetEvents();
        var count = 0;
        foreach (var pair in events.Select((@event, i) => new {Value = @event, Index = i}))
        {
            var value = pair.Value;
            var index = pair.Index;
            var fileUrl = $"{urlBase}{value.Name}";
            var eventString = $"{index},{fileUrl},{value.NotBefore},{value.NotAfter},{value.Md5},{value.Index}";
            dataString.Append(eventString).Append('\n');
            count++;
        }

        response = $"count={count}\n" +
                   "nexttime=1\n" +
                   $"{dataString}";

        return ValueTask.FromResult(response);
    }
}
```

Replace `Application/Game/Option/PlayCountQuery.cs` with:

```csharp
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Game.Option;

public record PlayCountQuery(long CardId) : IQuery<long>;

public class PlayCountQueryHandler : IQueryHandler<PlayCountQuery, long>
{
    private readonly ICardDbContext context;
    private readonly ILogger<PlayCountQueryHandler> logger;

    public PlayCountQueryHandler(ICardDbContext context, ILogger<PlayCountQueryHandler> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    public async ValueTask<long> Handle(PlayCountQuery request, CancellationToken cancellationToken)
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
```

Replace `Application/Game/Server/CertifyCommand.cs` with:

```csharp
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Domain.Config;
using Mediator;
using Microsoft.Extensions.Options;

namespace Application.Game.Server;

public record CertifyCommand(string? Gid, string? Mac, string? Random, string? Md5, string Host)
    : ICommand<string>;

public partial class CertifyCommandHandler : ICommandHandler<CertifyCommand, string>
{
    private readonly RelayConfig relayConfig;
    private readonly AuthConfig authConfig;

    public CertifyCommandHandler(IOptions<RelayConfig> relayOptions, IOptions<AuthConfig> authOptions)
    {
        relayConfig = relayOptions.Value;
        authConfig = authOptions.Value;
    }

    public ValueTask<string> Handle(CertifyCommand request, CancellationToken cancellationToken)
    {
        if (request.Gid == null)
        {
            return ValueTask.FromResult(QuitWithError(ErrorCode.ErrorNoGid));
        }

        if (request.Mac == null)
        {
            return ValueTask.FromResult(QuitWithError(ErrorCode.ErrorNoMac));
        }

        if (request.Random == null)
        {
            return ValueTask.FromResult(QuitWithError(ErrorCode.ErrorNoRandom));
        }

        if (request.Md5 == null)
        {
            return ValueTask.FromResult(QuitWithError(ErrorCode.ErrorNoHash));
        }

        if (!MacValid(request.Mac))
        {
            return ValueTask.FromResult(QuitWithError(ErrorCode.ErrorInvalidMac));
        }

        if (!Md5Valid(request.Md5))
        {
            return ValueTask.FromResult(QuitWithError(ErrorCode.ErrorInvalidHash));
        }

        var machine = new Machine
        {
            TenpoId = "1337",
            TenpoName = "GCLocalServer",
            Pref = "nesys",
            Location = "Local",
            Mac = request.Mac
        };
        if (authConfig.Enabled)
        {
            machine = authConfig.Machines.FirstOrDefault(m => m.Mac == request.Mac);
            if (machine is null)
            {
                return ValueTask.FromResult(QuitWithError(ErrorCode.ErrorInvalidMac));
            }
        }

        var ticket = string.Join(string.Empty,
            MD5.HashData(Encoding.UTF8.GetBytes(request.Gid)).Select(b => b.ToString("x2")));

        var response = $"host=card_id=7020392000147361,relay_addr={relayConfig.RelayServer},relay_port={relayConfig.RelayPort}\n" +
                       $"no={machine.TenpoId}\n" +
                       $"name={machine.TenpoName}\n" +
                       $"pref={machine.Pref}\n" +
                       $"addr={machine.Location}\n" +
                       "x-next-time=15\n" +
                       $"x-img=http://{request.Host}/news.png\n" +
                       $"x-ranking=http://{request.Host}/ranking/ranking.php\n" +
                       $"ticket={ticket}";

        return ValueTask.FromResult(response);
    }

    private static bool MacValid(string mac)
    {
        return MacRegex().IsMatch(mac);
    }

    private static bool Md5Valid(string md5)
    {
        return Md5Regex().IsMatch(md5);
    }

    private static string QuitWithError(ErrorCode errorCode)
    {
        return $"error={(int)errorCode}";
    }

    private enum ErrorCode
    {
        ErrorNoGid,
        ErrorNoMac,
        ErrorNoRandom,
        ErrorNoHash,
        ErrorInvalidMac,
        ErrorInvalidHash
    }

    [GeneratedRegex("^[a-fA-F0-9]{12}$")]
    private static partial Regex MacRegex();

    [GeneratedRegex("^[a-fA-F0-9]{32}$")]
    private static partial Regex Md5Regex();
}
```

- [ ] **Step 7: Convert both cross-cutting concerns to native pipeline APIs**

Replace `Application/Common/Behaviours/LoggingBehaviour.cs` with:

```csharp
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviours;

public sealed class LoggingBehaviour<TMessage, TResponse> : MessagePreProcessor<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    private readonly ILogger<TMessage> logger;

    public LoggingBehaviour(ILogger<TMessage> logger)
    {
        this.logger = logger;
    }

    protected override ValueTask Handle(TMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Received request: {RequestName}, content: {Request}",
            typeof(TMessage).Name,
            message);

        return ValueTask.CompletedTask;
    }
}
```

Replace `Application/Common/Behaviours/UnhandledExceptionBehaviour.cs` with:

```csharp
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviours;

public sealed class UnhandledExceptionBehaviour<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    private readonly ILogger<TMessage> logger;

    public UnhandledExceptionBehaviour(ILogger<TMessage> logger)
    {
        this.logger = logger;
    }

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(message, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unhandled Exception for Request {Name} {@Request}",
                typeof(TMessage).Name,
                message);

            throw;
        }
    }
}
```

- [ ] **Step 8: Perform the atomic package and registration switch**

In `Application/Application.csproj`, remove the MediatR reference and leave this final package configuration:

```xml
<PackageReference Include="Mediator.Abstractions" Version="3.0.2" />
<PackageReference Include="Mediator.SourceGenerator" Version="3.0.2">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

In `Application/DependencyInjection.cs`, remove `System.Reflection` and `MediatR`, add `using Mediator;`, and replace registration with:

```csharp
services.AddMediator(options =>
{
    options.Assemblies = [typeof(DependencyInjection)];
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.PipelineBehaviors =
    [
        typeof(LoggingBehaviour<,>),
        typeof(UnhandledExceptionBehaviour<,>)
    ];
});
```

Delete the old manual registration:

```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
```

Keep every Quartz job and trigger registration unchanged.

- [ ] **Step 9: Switch the controller sender and remove old wrappers**

In `MainServer/Controllers/BaseController.cs`, replace only:

```csharp
using MediatR;
```

with:

```csharp
using Mediator;
```

Keep the cached `ISender` property and every controller `Mediator.Send(...)` call unchanged. Delete `Application/Interfaces/IRequestWrapper.cs`; no alias or bridge replaces it.

- [ ] **Step 10: Prove the mechanical sweep is complete before compiling**

Run:

```powershell
$legacyMatches = @(rg -n "\bMediatR\b|IRequestWrapper|IRequestHandlerWrapper|AddMediatR" Application MainServer Tests)
if ($legacyMatches.Count -ne 0) { $legacyMatches; throw 'Legacy mediator symbols remain.' }

$taskHandlers = @(rg -n -g '*.cs' "public (override )?(async )?Task<.*> Handle\(" Application)
if ($taskHandlers.Count -ne 0) { $taskHandlers; throw 'Mediator handlers still return Task.' }
```

Expected: both commands return no matches. `Task` remains valid in private helpers and Quartz `Execute` methods.

- [ ] **Step 11: Run the native migration tests and full suite**

Run:

```powershell
dotnet test Tests/XmlSerializationTests/XmlSerializationTests.csproj -c Release --filter "FullyQualifiedName~MediatorMigrationTests"
dotnet test GC-local-server-rewrite.sln -c Release
```

Expected: 5 migration tests pass; the full suite reports 19 passed, 0 failed. Any `MSG` source-generator diagnostic must be investigated rather than suppressed.

- [ ] **Step 12: Inspect emitted generated registration**

Run:

```powershell
$generatedDir = Join-Path $env:TEMP ("gc-mediator-generated-" + [Guid]::NewGuid().ToString("N"))
dotnet build Application/Application.csproj -c Release -p:EmitCompilerGeneratedFiles=true -p:CompilerGeneratedFilesOutputPath=$generatedDir
rg -n "AddMediator|ValidateAdminLoginQuery|CertifyCommand|LoggingBehaviour|UnhandledExceptionBehaviour" $generatedDir
```

Expected: build exits 0, and generated source contains `AddMediator`, both representative handlers, and both pipeline behavior registrations.

- [ ] **Step 13: Review and commit the atomic runtime migration**

Run:

```powershell
git diff --check
git status --short
git add -- Application MainServer/Controllers/BaseController.cs Tests/XmlSerializationTests/MediatorMigrationTests.cs
git diff --cached --check
git commit -m "Migrate dispatch to native Mediator"
```

Expected: the commit contains only Application, BaseController, and migration-test changes. Do not stage `CLAUDE.md`, `.claude/`, or `AGENTS.md`.

### Task 3: Update Active Guidance and Run Release Verification

**Files:**
- Modify without staging unrelated content: `AGENTS.md:5,18`
- Modify without staging unrelated content: `CLAUDE.md:39-49,81`

**Interfaces:**
- Consumes: the final native contract/base/DI names from Task 2.
- Produces: current contributor guidance and release-level evidence that no MediatR runtime artifact remains.

- [ ] **Step 1: Update repository guidance with the final native names**

In `AGENTS.md`, use these statements in the existing paragraphs:

```markdown
`Application/` contains native Mediator commands, queries, jobs, and Mapperly mappers.
```

```markdown
Name native Mediator messages by intent, such as `GetMusicListQuery` or `SetPlayerNameCommand`, and implement the matching query or command handler contract.
```

In `CLAUDE.md`, replace only mediator-specific wording with:

```markdown
**Layered architecture with CQRS via source-generated Mediator:**

MainServer (ASP.NET Core host)
  ├── Controllers dispatch to native Mediator handlers

Application (business logic)
  ├── Game/Card/Read/   — native query handlers
  ├── Game/Card/Write/  — native command handlers
```

Replace the request-flow bullet with:

```markdown
- **Request flow:** HTTP requests hit controllers, which send native Mediator commands or queries. Card-dependent handlers inherit `QueryHandlerBase<TQuery, T>` or `CommandHandlerBase<TCommand, T>` for `ICardDependencyAggregate`; lightweight handlers implement `IQueryHandlerWrapper<TQuery, T>` or `ICommandHandlerWrapper<TCommand, T>` directly. `AddApplication()` configures source-generated, scoped dispatch plus logging and exception pipeline behaviors.
```

Do not rewrite or stage the pre-existing publish, DatParser, configuration, or gotcha additions in `CLAUDE.md`.

- [ ] **Step 2: Verify active source and guidance contain no legacy namespace or wrapper**

Run:

```powershell
$legacyMatches = @(rg -n "\bMediatR\b|IRequestWrapper|IRequestHandlerWrapper|AddMediatR" Application MainServer Infrastructure Domain Shared Tests WebUI GCRelayServer DatParser AGENTS.md CLAUDE.md)
if ($legacyMatches.Count -ne 0) { $legacyMatches; throw 'Legacy mediator symbols remain in active source or guidance.' }
```

Expected: no matches. Historical mentions in the committed design and implementation plan are allowed because they describe the dependency being removed.

- [ ] **Step 3: Verify the resolved dependency graph**

Run:

```powershell
$packages = dotnet list Application/Application.csproj package --include-transitive
$packages
if ($packages | Select-String -Pattern '\bMediatR\b') { throw 'Legacy MediatR package is still resolved.' }
if (-not ($packages | Select-String -Pattern 'Mediator\.Abstractions\s+3\.0\.2')) { throw 'Mediator.Abstractions 3.0.2 is missing.' }
```

Expected: `Mediator.Abstractions` and `Mediator.SourceGenerator` are direct 3.0.2 references, and no MediatR package is listed.

- [ ] **Step 4: Run fresh Release build and test gates**

Run:

```powershell
dotnet restore GC-local-server-rewrite.sln
dotnet build GC-local-server-rewrite.sln -c Release --no-restore
dotnet test GC-local-server-rewrite.sln -c Release --no-build
```

Expected: restore and build exit 0; tests report 19 passed, 0 failed. Existing NU1901/NU1903 vulnerability warnings may be reported, but there must be no source-generator, compile, or test failures.

- [ ] **Step 5: Publish MainServer into a fresh directory and inspect artifacts**

Run:

```powershell
$publishDir = Join-Path $env:TEMP ("gc-local-server-mediator-" + [Guid]::NewGuid().ToString("N"))
dotnet publish MainServer/MainServer.csproj -c Release -r win-x64 --self-contained true -o $publishDir

$legacyArtifacts = @(Get-ChildItem -LiteralPath $publishDir -Recurse -Filter 'MediatR*.dll')
if ($legacyArtifacts.Count -ne 0) { throw "Legacy MediatR artifacts found: $($legacyArtifacts.FullName -join ', ')" }

$nativeArtifacts = @(Get-ChildItem -LiteralPath $publishDir -Recurse -Filter 'Mediator.dll')
if ($nativeArtifacts.Count -ne 1) { throw "Expected one Mediator.dll, found $($nativeArtifacts.Count)." }

Write-Output "PUBLISH_DIR=$publishDir"
Write-Output "NATIVE_MEDIATOR=$($nativeArtifacts[0].FullName)"
```

Expected: publish exits 0, no `MediatR*.dll` exists, and exactly one native `Mediator.dll` is present. Keep the unique temporary directory as verification evidence; do not write into the repository's `publish/` directory.

- [ ] **Step 6: Review final diff ownership and whitespace**

Run:

```powershell
git diff --check
git status --short
git diff -- Application MainServer Tests AGENTS.md CLAUDE.md
```

Expected: no whitespace errors. Runtime code/tests are committed by Tasks 1-2. `CLAUDE.md`, `.claude/`, and `AGENTS.md` remain unstaged unless the user explicitly authorizes combining their pre-existing changes into a later documentation commit.

- [ ] **Step 7: Record the implementation evidence in the handoff**

Report:

```text
- Native packages and resolved versions
- Query/command/handler counts migrated
- Focused and full test totals
- Release build and fresh publish results
- Generated registration inspection result
- Legacy source/package/artifact scans
- Existing NuGet vulnerability warnings
- Unstaged pre-existing documentation and .claude changes
```

Do not create a documentation commit in this task without explicit authorization, because both active guidance files predate the migration as uncommitted user-owned work.
