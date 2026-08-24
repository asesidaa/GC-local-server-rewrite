# MainServer Online Matching Redesign Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace poll-count-based online matching with an event-scoped, idempotent, elapsed-time room aggregate that validates `8705`, `8961`, and `8709` while preserving the native XML contract.

**Architecture:** Domain owns transient room state and deterministic phase transitions. Application owns dedicated XML DTOs, response serialization, Mediator handlers, and service contracts. Infrastructure coordinates rooms and identity indexes under one lock, supplies `TimeProvider`, generates nonzero uint IDs, and performs cleanup. MainServer binds validated `OnlineMatching` options. A dedicated xUnit project exercises the real aggregate, coordinator, handlers, and XML.

**Tech Stack:** .NET 10, C# 14, native Mediator 3.0.2, Mapperly-free manual protocol mapping, Microsoft options/DI, Quartz.NET 3.19.0, xUnit v3 3.2.2.

## Global Constraints

- Treat `doc/multiplayer/game471-multiplayer-protocol.md` as the protocol authority.
- Preserve card request types `8705`, `8961`, and `8709`, controller routing, and XML field names.
- Keep MainServer and GCRelayServer as separate runtime processes.
- Use provisional configurable defaults: 99-second search, 40-second formation, 10-second poll.
- Derive all countdown fields from `TimeProvider`; never decrement them per request.
- Scope room admission by `event_id`; cap and validate entry numbers in `0..3`.
- Use nonzero `uint` matching IDs and retain removed-ID tombstones for the configured guard period.
- Do not add a database migration or durable result analytics.
- Do not modify relay behavior in this plan.
- Use test-first red/green cycles and commit each task independently.

## File Map

- Create `Tests/MultiplayerTests/MultiplayerTests.csproj`: focused multiplayer xUnit project.
- Create `Tests/MultiplayerTests/ManualTimeProvider.cs`: deterministic clock used by both plans.
- Create `Tests/MultiplayerTests/MainServer/OnlineMatchRoomTests.cs`: aggregate phase/timing tests.
- Create `Tests/MultiplayerTests/MainServer/OnlineMatchingServiceTests.cs`: event, identity, ID, cleanup, and concurrency tests.
- Create `Tests/MultiplayerTests/MainServer/OnlineMatchingHandlerTests.cs`: XML and handler validation tests.
- Modify `GC-local-server-rewrite.slnx`: include the focused test project.
- Create `Domain/Config/OnlineMatchingConfig.cs`: validated timing, capacity, retention, and ID-reuse options.
- Create `Domain/Enums/OnlineMatchPhase.cs`: `Searching`, `Forming`, `Ready`, and `Closed`.
- Create `Domain/Models/OnlineMatching/OnlineMatchParticipant.cs`: stable participant/profile state.
- Create `Domain/Models/OnlineMatching/OnlineMatchResultSubmission.cs`: parsed result state.
- Create `Domain/Models/OnlineMatching/OnlineMatchRoom.cs`: deterministic aggregate.
- Create `Domain/Models/OnlineMatching/OnlineMatchRoomSnapshot.cs`: immutable service/serialization view.
- Delete `Application/Game/Card/OnlineMatching/MatchEntry.cs`: superseded mutable response model.
- Delete `Application/Game/Card/OnlineMatching/MatchRoom.cs`: superseded poll-count aggregate.
- Delete `Application/Game/Card/OnlineMatching/MatchRoomSnapshot.cs`: superseded serializer-coupled snapshot.
- Delete `Application/Dto/Game/OnlineMatchEntryDto.cs`: superseded request/response catch-all DTO.
- Delete `Application/Mappers/OnlineMatchMapper.cs`: mapping becomes explicit at the protocol boundary.
- Create `Application/Dto/Game/OnlineMatchingEntryRequestDto.cs`: `8705` request only.
- Create `Application/Dto/Game/OnlineMatchingUpdateRequestDto.cs`: `8961` request only.
- Create `Application/Dto/Game/OnlineMatchingResultRequestDto.cs`: `8709` request only.
- Create `Application/Dto/Game/OnlineMatchingRecordDto.cs`: response record only.
- Create `Application/Game/Card/OnlineMatching/OnlineMatchingXml.cs`: checked DTO conversion and snapshot serialization.
- Create `Application/Game/Card/OnlineMatching/OnlineMatchingServiceResult.cs`: typed service failures.
- Modify `Application/Interfaces/IOnlineMatchingService.cs`: explicit enter/update/result/cleanup operations.
- Create `Application/Interfaces/IMatchingIdGenerator.cs`: injectable uint ID source.
- Create `Infrastructure/Services/RandomMatchingIdGenerator.cs`: cryptographic nonzero uint generator.
- Rewrite `Infrastructure/Services/OnlineMatchingService.cs`: locked room coordinator and indexes.
- Rewrite the three matching command handlers: dedicated DTO parsing and service validation.
- Modify `Infrastructure/DependencyInjection.cs`: register clock, ID generator, and service.
- Modify `MainServer/Program.cs`: bind and validate `OnlineMatchingConfig`.
- Modify `MainServer/Configurations/matching.json`: add documented defaults.
- Modify `Application/Jobs/CleanupOnlineMatchesJob.cs`: retain scheduled cleanup with updated semantics/logging.

---

### Task 1: Add the Focused Multiplayer Test Harness

**Files:**

- Create: `Tests/MultiplayerTests/MultiplayerTests.csproj`
- Create: `Tests/MultiplayerTests/ManualTimeProvider.cs`
- Modify: `GC-local-server-rewrite.slnx`

- [ ] **Step 1: Create the test project**

Use the repository's xUnit v3 package pattern and reference the three MainServer layers:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <OutputType>Exe</OutputType>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="coverlet.collector" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit.v3" />
    <PackageReference Include="xunit.runner.visualstudio" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\Application\Application.csproj" />
    <ProjectReference Include="..\..\Domain\Domain.csproj" />
    <ProjectReference Include="..\..\Infrastructure\Infrastructure.csproj" />
  </ItemGroup>
  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Add a deterministic clock**

```csharp
namespace MultiplayerTests;

public sealed class ManualTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    private DateTimeOffset utcNow = utcNow;

    public override DateTimeOffset GetUtcNow() => utcNow;

    public void Advance(TimeSpan elapsed) => utcNow += elapsed;
}
```

- [ ] **Step 3: Add the project to the solution**

Add `Tests/MultiplayerTests/MultiplayerTests.csproj` inside the existing `/Tests/` folder in `GC-local-server-rewrite.slnx`.

- [ ] **Step 4: Verify the empty harness builds**

Run: `dotnet test Tests/MultiplayerTests/MultiplayerTests.csproj -c Release`

Expected: build succeeds and xUnit reports no failed tests.

- [ ] **Step 5: Commit**

```powershell
git add -- Tests/MultiplayerTests/MultiplayerTests.csproj Tests/MultiplayerTests/ManualTimeProvider.cs GC-local-server-rewrite.slnx
git commit -m "Add multiplayer test harness"
```

---

### Task 2: Model Deterministic Room Phases in Domain

**Files:**

- Create: `Tests/MultiplayerTests/MainServer/OnlineMatchRoomTests.cs`
- Create: `Domain/Config/OnlineMatchingConfig.cs`
- Create: `Domain/Enums/OnlineMatchPhase.cs`
- Create: `Domain/Models/OnlineMatching/OnlineMatchParticipant.cs`
- Create: `Domain/Models/OnlineMatching/OnlineMatchResultSubmission.cs`
- Create: `Domain/Models/OnlineMatching/OnlineMatchRoomSnapshot.cs`
- Create: `Domain/Models/OnlineMatching/OnlineMatchRoom.cs`

- [ ] **Step 1: Write failing phase and timing tests**

Cover these exact behaviors with a fixed starting instant:

```csharp
[Fact]
public void FirstParticipant_UsesSearchDeadlineAnd99_10_89Timing();

[Fact]
public void SecondParticipant_StartsOneFortySecondFormationWindow();

[Fact]
public void ThirdParticipant_DoesNotRestartFormationDeadline();

[Fact]
public void FourthParticipant_BecomesReadyImmediately();

[Fact]
public void SearchDeadline_WithOneParticipant_BecomesReady();

[Fact]
public void FormationDeadline_WithThreeParticipants_BecomesReady();

[Fact]
public void SnapshotTiming_UsesCeilingAndDoesNotMutateState();
```

Assert status `1` while active, status `3` when ready, stable entries, and a shared zero timing tuple after readiness.

- [ ] **Step 2: Run tests to verify RED**

Run: `dotnet test Tests/MultiplayerTests/MultiplayerTests.csproj -c Release --filter FullyQualifiedName~OnlineMatchRoomTests`

Expected: compilation fails because the Domain types do not exist.

- [ ] **Step 3: Add validated options and phase enum**

`OnlineMatchingConfig` has section name `OnlineMatching`, defaults
`99/40/10/600/120/600/4`, and data-annotation ranges that require positive
durations and `MaxPlayers` in `1..4`.

```csharp
public enum OnlineMatchPhase
{
    Searching,
    Forming,
    Ready,
    Closed
}
```

- [ ] **Step 4: Implement participant, result, and immutable snapshot records**

Use exact-width `uint` protocol integers after checked DTO conversion. Keep
`CardId` as the server's `long` outer-card identity and keep entry start as
`DateTimeOffset`.

The snapshot exposes:

```csharp
public sealed record OnlineMatchTiming(uint Timeout, uint Wait, uint Remaining);

public sealed record OnlineMatchRoomSnapshot(
    uint MatchingId,
    uint EventId,
    OnlineMatchPhase Phase,
    OnlineMatchTiming Timing,
    IReadOnlyList<OnlineMatchParticipantSnapshot> Participants);
```

- [ ] **Step 5: Implement the aggregate**

The room constructor adds entry zero and sets the search deadline. `TryJoin`
assigns the lowest unused entry, starts formation only when count changes from
one to two, and makes the room ready at configured capacity. `Advance(now)`
uses the current deadline. `CreateSnapshot(now)` computes:

```csharp
var timeout = phase is OnlineMatchPhase.Searching or OnlineMatchPhase.Forming
    ? (uint)Math.Max(0, Math.Ceiling((deadline - now).TotalSeconds))
    : 0;
var wait = Math.Min((uint)options.PollIntervalSeconds, timeout);
var remaining = timeout - wait;
```

- [ ] **Step 6: Run tests to verify GREEN**

Run: `dotnet test Tests/MultiplayerTests/MultiplayerTests.csproj -c Release --filter FullyQualifiedName~OnlineMatchRoomTests`

Expected: all room tests pass.

- [ ] **Step 7: Commit**

```powershell
git add -- Domain/Config/OnlineMatchingConfig.cs Domain/Enums/OnlineMatchPhase.cs Domain/Models/OnlineMatching Tests/MultiplayerTests/MainServer/OnlineMatchRoomTests.cs
git commit -m "Add deterministic online match aggregate"
```

---

### Task 3: Replace the In-Memory Coordinator

**Files:**

- Create: `Tests/MultiplayerTests/MainServer/OnlineMatchingServiceTests.cs`
- Create: `Application/Game/Card/OnlineMatching/OnlineMatchingServiceResult.cs`
- Create: `Application/Interfaces/IMatchingIdGenerator.cs`
- Modify: `Application/Interfaces/IOnlineMatchingService.cs`
- Create: `Infrastructure/Services/RandomMatchingIdGenerator.cs`
- Rewrite: `Infrastructure/Services/OnlineMatchingService.cs`

- [ ] **Step 1: Write failing coordinator tests**

Test through `IOnlineMatchingService`/`OnlineMatchingService`:

```csharp
[Fact]
public void EnterRetry_ReturnsSameMatchingIdAndEntry();

[Fact]
public void DifferentEvents_CreateDifferentRooms();

[Fact]
public void ConflictingCardOrMachine_IsRejected();

[Fact]
public void EntryNumbers_AreStableAndCapacityIsFour();

[Fact]
public void Update_ValidatesCardEventAndEntryDependentAction();

[Fact]
public void UnevenPolling_DoesNotAccelerateCountdown();

[Fact]
public void Cleanup_UsesActivityAndTerminalRetention();

[Fact]
public void RemovedMatchingId_IsGuardedAgainstImmediateReuse();

[Fact]
public async Task ConcurrentAdmissions_DoNotOverfillOrDuplicateEntries();
```

Use a deterministic ID generator returning a known sequence including `0`, an
active ID, and a tombstoned ID so rejection/retry paths are explicit.

- [ ] **Step 2: Run tests to verify RED**

Run: `dotnet test Tests/MultiplayerTests/MultiplayerTests.csproj -c Release --filter FullyQualifiedName~OnlineMatchingServiceTests`

Expected: compilation fails on the new service contract.

- [ ] **Step 3: Define explicit service operations and failures**

The interface exposes synchronous transient operations:

```csharp
OnlineMatchingServiceResult<OnlineMatchRoomSnapshot> Enter(OnlineMatchParticipant participant);
OnlineMatchingServiceResult<OnlineMatchRoomSnapshot> Update(
    uint matchingId, uint eventId, long cardId, uint action, uint messageId);
OnlineMatchingServiceResult<bool> SubmitResult(
    long cardId, OnlineMatchResultSubmission submission);
void CleanupStaleRooms();
```

Use a small generic result with `Value`, `Error`, and `IsSuccess`; define stable
error codes for invalid identity, conflict, room not found, membership, action,
capacity, and conflicting result.

- [ ] **Step 4: Implement cryptographic uint ID generation**

`RandomMatchingIdGenerator.Next()` fills four bytes using
`RandomNumberGenerator.Fill` and returns the resulting `uint`. The coordinator
rejects zero and retries if the ID is active or tombstoned.

- [ ] **Step 5: Implement the locked coordinator**

Use one `Lock` around ordinary dictionaries for atomic room selection,
identity-index maintenance, phase advancement, and cleanup. Under the lock:

- advance rooms before deciding whether they are joinable;
- index active identities by card and machine;
- return an existing snapshot for an exact retry;
- choose the oldest joinable room with matching event ID;
- update only the caller's message ID after action validation;
- close/release identities when all results arrive;
- retain removed IDs until `MatchingIdReuseGuardSeconds` elapses.

Do not call `DateTime.UtcNow`; obtain one `now` value per public operation from
the injected `TimeProvider`.

- [ ] **Step 6: Run tests to verify GREEN**

Run: `dotnet test Tests/MultiplayerTests/MultiplayerTests.csproj -c Release --filter FullyQualifiedName~OnlineMatchingServiceTests`

Expected: all coordinator tests pass repeatedly.

- [ ] **Step 7: Commit**

```powershell
git add -- Application/Game/Card/OnlineMatching/OnlineMatchingServiceResult.cs Application/Interfaces/IOnlineMatchingService.cs Application/Interfaces/IMatchingIdGenerator.cs Infrastructure/Services/OnlineMatchingService.cs Infrastructure/Services/RandomMatchingIdGenerator.cs Tests/MultiplayerTests/MainServer/OnlineMatchingServiceTests.cs
git commit -m "Replace online matching room coordinator"
```

---

### Task 4: Split XML Requests from Response Records

**Files:**

- Create: `Tests/MultiplayerTests/MainServer/OnlineMatchingXmlTests.cs`
- Create: `Application/Dto/Game/OnlineMatchingEntryRequestDto.cs`
- Create: `Application/Dto/Game/OnlineMatchingUpdateRequestDto.cs`
- Create: `Application/Dto/Game/OnlineMatchingResultRequestDto.cs`
- Create: `Application/Dto/Game/OnlineMatchingRecordDto.cs`
- Create: `Application/Game/Card/OnlineMatching/OnlineMatchingXml.cs`
- Delete: `Application/Dto/Game/OnlineMatchEntryDto.cs`
- Delete: `Application/Mappers/OnlineMatchMapper.cs`
- Delete: `Application/Game/Card/OnlineMatching/MatchEntry.cs`
- Delete: `Application/Game/Card/OnlineMatching/MatchRoom.cs`
- Delete: `Application/Game/Card/OnlineMatching/MatchRoomSnapshot.cs`

- [ ] **Step 1: Write failing XML boundary tests**

Deserialize representative `8705`, `8961`, and `8709` XML fragments into
their dedicated DTOs. Serialize a two-record snapshot and assert exact element
names, record IDs, stable entry numbers, timestamps, status, and timing fields.
Add checked-conversion cases for zero/negative/out-of-uint-range machine,
event, matching, action, and result values.

- [ ] **Step 2: Run tests to verify RED**

Run: `dotnet test Tests/MultiplayerTests/MultiplayerTests.csproj -c Release --filter FullyQualifiedName~OnlineMatchingXmlTests`

Expected: compilation fails because the dedicated DTOs and converter do not exist.

- [ ] **Step 3: Add request-only DTOs**

Use `[XmlElement]` names from the protocol reference. Do not carry response
defaults on request DTOs. `OnlineMatchingResultRequestDto` includes all 15
confirmed request fields.

- [ ] **Step 4: Add the response-only DTO and explicit conversion**

`OnlineMatchingRecordDto` contains every response field, including
`entry_start`, but no request-only `action`. `OnlineMatchingXml` performs
checked conversion to Domain types and manually maps snapshots back to records.
Format entry start as `yyyy/MM/dd HH:mm:ss`, matching existing server behavior.

- [ ] **Step 5: Remove superseded mutable/mapped models**

Delete the catch-all DTO, Mapperly mapper, and old Application aggregate files
after all call sites compile against the new boundary.

- [ ] **Step 6: Run tests to verify GREEN**

Run: `dotnet test Tests/MultiplayerTests/MultiplayerTests.csproj -c Release --filter FullyQualifiedName~OnlineMatchingXmlTests`

Expected: all conversion and XML assertions pass.

- [ ] **Step 7: Commit**

```powershell
git add -- Application/Dto/Game Application/Game/Card/OnlineMatching Application/Mappers/OnlineMatchMapper.cs Tests/MultiplayerTests/MainServer/OnlineMatchingXmlTests.cs
git commit -m "Split online matching protocol DTOs"
```

---

### Task 5: Rewire the Three Mediator Handlers

**Files:**

- Create: `Tests/MultiplayerTests/MainServer/OnlineMatchingHandlerTests.cs`
- Modify: `Application/Game/Card/OnlineMatching/StartOnlineMatchingCommand.cs`
- Modify: `Application/Game/Card/OnlineMatching/UpdateOnlineMatchingCommand.cs`
- Modify: `Application/Game/Card/OnlineMatching/UploadOnlineMatchingResultCommand.cs`

- [ ] **Step 1: Write failing handler tests**

Use a recording fake `IOnlineMatchingService` to assert:

- start parses `8705`, injects the outer card ID, and does not override timing;
- update forwards `action`, `event_id`, `matching_id`, and `message_id`;
- invalid conversion and service failures return failed `ServiceResult` values;
- result parses all fields, associates the outer card ID, and serializes status
  `1` only after a successful submission.

- [ ] **Step 2: Run tests to verify RED**

Run: `dotnet test Tests/MultiplayerTests/MultiplayerTests.csproj -c Release --filter FullyQualifiedName~OnlineMatchingHandlerTests`

Expected: old handlers fail the new assertions or do not compile.

- [ ] **Step 3: Rewrite start and update handlers**

Start uses `OnlineMatchingEntryRequestDto`, checked conversion, and
`matchingService.Enter`. Update uses `OnlineMatchingUpdateRequestDto` and
`matchingService.Update`; it no longer relies on a response DTO's default
fields. Map stable service errors to `ServiceError.CustomMessage` and preserve
successful list XML.

- [ ] **Step 4: Rewrite result handler**

Deserialize `OnlineMatchingResultRequestDto`, convert it to
`OnlineMatchResultSubmission`, call `SubmitResult`, and serialize the existing
status-1 response only on success. Identical retries remain successful through
the service.

- [ ] **Step 5: Run tests to verify GREEN**

Run: `dotnet test Tests/MultiplayerTests/MultiplayerTests.csproj -c Release --filter FullyQualifiedName~OnlineMatchingHandlerTests`

Expected: all handler tests pass.

- [ ] **Step 6: Commit**

```powershell
git add -- Application/Game/Card/OnlineMatching Tests/MultiplayerTests/MainServer/OnlineMatchingHandlerTests.cs
git commit -m "Validate online matching card requests"
```

---

### Task 6: Bind Configuration and Scheduled Cleanup

**Files:**

- Create: `Tests/MultiplayerTests/MainServer/OnlineMatchingRegistrationTests.cs`
- Modify: `Infrastructure/DependencyInjection.cs`
- Modify: `MainServer/Program.cs`
- Modify: `MainServer/Configurations/matching.json`
- Modify: `Application/Jobs/CleanupOnlineMatchesJob.cs`

- [ ] **Step 1: Write failing registration/configuration tests**

Build a service collection with valid options and assert singleton resolution
of `TimeProvider`, `IMatchingIdGenerator`, and `IOnlineMatchingService`. Bind
the checked-in JSON and assert exact `99/40/10/600/120/600/4` defaults. Add an
invalid `MaxPlayers = 5` options-validation test.

- [ ] **Step 2: Run tests to verify RED**

Run: `dotnet test Tests/MultiplayerTests/MultiplayerTests.csproj -c Release --filter FullyQualifiedName~OnlineMatchingRegistrationTests`

Expected: registration/options assertions fail.

- [ ] **Step 3: Register clock and ID generator**

In `AddInfrastructure`, use `TryAddSingleton(TimeProvider.System)`, register
`IMatchingIdGenerator` as singleton, and retain `IOnlineMatchingService` as a
singleton.

- [ ] **Step 4: Bind validated options and update JSON**

In `MainServer/Program.cs` use:

```csharp
builder.Services.AddOptions<OnlineMatchingConfig>()
    .Bind(builder.Configuration.GetSection(OnlineMatchingConfig.SECTION))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

Add the approved values to `matching.json` while retaining the existing Relay
section unchanged.

- [ ] **Step 5: Keep Quartz cleanup as the scheduler boundary**

The job continues calling `CleanupStaleRooms()` once per invocation. Adjust its
log message to describe phase/idle/tombstone cleanup without adding a second
timer or clock.

- [ ] **Step 6: Run focused and full MainServer-layer tests**

Run: `dotnet test Tests/MultiplayerTests/MultiplayerTests.csproj -c Release`

Run: `dotnet test Tests/XmlSerializationTests/XmlSerializationTests.csproj -c Release`

Expected: all tests pass.

- [ ] **Step 7: Commit**

```powershell
git add -- Infrastructure/DependencyInjection.cs MainServer/Program.cs MainServer/Configurations/matching.json Application/Jobs/CleanupOnlineMatchesJob.cs Tests/MultiplayerTests/MainServer/OnlineMatchingRegistrationTests.cs
git commit -m "Configure online matching lifecycle"
```

---

### Task 7: Verify the MainServer Redesign

**Files:**

- Modify only if verification exposes a defect in files owned by Tasks 1-6.

- [ ] **Step 1: Run formatting verification**

Run: `dotnet format GC-local-server-rewrite.slnx --verify-no-changes`

Expected: no formatting changes required. If it reports only task-owned files,
run `dotnet format GC-local-server-rewrite.slnx`, inspect the diff, and commit
the mechanical corrections with the affected task.

- [ ] **Step 2: Run the complete solution test suite**

Run: `dotnet test GC-local-server-rewrite.slnx -c Release`

Expected: all tests pass.

- [ ] **Step 3: Run the Release build**

Run: `dotnet build GC-local-server-rewrite.slnx -c Release --no-restore`

Expected: zero warnings introduced by matching changes and zero errors.

- [ ] **Step 4: Inspect scope and protocol-sensitive diffs**

Run: `git status --short`

Run: `git diff HEAD~6 --stat`

Run: `rg -n "MatchRemainingTime--|MatchTimeout = 20|MatchWaitTime = 5|DateTime.UtcNow" Application/Game/Card/OnlineMatching Infrastructure/Services/OnlineMatchingService.cs Domain/Models/OnlineMatching`

Expected: no poll-count decrement, hard-coded `20/5/5`, or direct wall-clock read remains in the redesigned path; no unrelated files are modified.

- [ ] **Step 5: Record the verification boundary**

Do not claim official-service timing parity or cabinet acceptance. Record that
the next proof is a timestamped official-service capture and real multi-cabinet
session testing after the relay plan is complete.
